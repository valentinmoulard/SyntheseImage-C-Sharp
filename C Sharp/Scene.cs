using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using System.Threading;

namespace Synthese_Image
{
    class Scene
    {
        public string name;
        public Sphere[] spheres;
        public List<Box> boites;
        public Camera camera;
        public Light light;
        Random random = new Random();

        public Scene(string n, Sphere[] s, Camera c, Light l, List<Box> b)
        {
            name = n;
            spheres = s;
            camera = c;
            light = l;

            boites = b;
        }

        public void DrawScene()
        {
            Tree Arbre = ConstructTree(boites);
            Vector3[,] pixMat = new Vector3[camera.width, camera.height];
            //nb_rayon_rebond est le nombre de rayon envoyé par pixel pour éviter le bruit pour les zones ombrées (car les rebond se font aléatoirement)
            int nb_rayon_rebond = 10;
            for (int y = 0; y < camera.height; y++)
            {
                for (int x = 0; x < camera.width; x++)
                {
                    //pour les spheres

                    Vector3 couleur_rayon = new Vector3(0.0f, 0.0f, 0.0f);
                    for (int c = 0; c < nb_rayon_rebond; c++)
                    {
                        Vector3 start = new Vector3(camera.center.X + x, camera.center.Y + y, camera.center.Z);
                        Vector3 direction = Vector3.Subtract(start, camera.focus);
                        direction = Vector3.Normalize(direction);
                        Ray r = new Ray(start, direction);
                        //couleur_rayon = Vector3.Add(Radiance(r, 0), couleur_rayon);
                        couleur_rayon = Vector3.Add(Radiance(r, 0, Arbre), couleur_rayon);
                    }
                    pixMat[x, y] = couleur_rayon / nb_rayon_rebond;
                }
            }
            ImagePPM img = new ImagePPM(name, camera.width, camera.height).FromMatrix(pixMat);
            img.ToPPM();
        }

        //public Vector3 Radiance(Ray ray, int stop)
        public Vector3 Radiance(Ray ray, int stop, Tree arbre)
        {
            Vector3 color = new Vector3(0, 0, 0);
            float nb_rebond = 10.0f;
            //tant qu'on a pas eu 100 rebonds
            if (stop != 10)
            {
                //recherche d'intersection pour sphere
                //ResIntersect resInter = Intersects(ray);

                //recherche d'intersection pour sphere AVEC structure acceleratrice
                ResIntersect resInter = IntersectTree(arbre, ray);


                //si on a une intersection
                if (resInter.t != -1)
                {
                    //interpoint, coordonnées du point t'intersection
                    Vector3 interPoint = Vector3.Add(ray.point, Vector3.Multiply(ray.direction, resInter.t));
                    //normal, vecteur normal a la surface de la sphere
                    Vector3 normal = Vector3.Subtract(interPoint, resInter.sph.center);
                    normal = Vector3.Normalize(normal);
                    //ajout du petit décalage pour ne pas avoir d'interesection d'un point d'une sphere avec un autre point de la sphere
                    interPoint = Vector3.Add(interPoint, Vector3.Multiply(normal, 0.1f));

                    //selon le materiau
                    switch (resInter.sph.material.type)
                    {
                        //DIFFUS
                        case MaterialType.Difuse:
                            //directionToLight, vecteur poitant vers la lumiere à partir du point d'intersection sur la sphere
                            Vector3 directionToLight = Vector3.Subtract(light.origin, interPoint);
                            //création d'un Rayon à partir de cette direction
                            ray = new Ray(interPoint, directionToLight);
                            //cherche s'il y a un obstacle entre la lumiere et le point de la sphere
                            ResIntersect resInterL = Intersects(ray);
                            //lightPower contiendra les albedo additionnés à chaque rebond
                            Vector3 lightPower = new Vector3(0.0f, 0.0f, 0.0f);

                            //s'il n'y a pas d'obstacle et s'il ne s'agit pas de la lampe
                            if (!(resInterL.t != -1 && resInterL.t <= 1.0f && resInterL.sph.material.type != MaterialType.Light))
                            {
                                lightPower = ReceiveLight(light, interPoint, resInter.sph);
                            }

                            //calcul des coordonnées d'un point d'une sphere fictive sur le point d'intersection de la shpere pour calculer une nouvelle direction
                            Vector3 mini_sphere_coord = RandomVector(interPoint);

                            //création d'une base à partir du vecteur normal au point d'intersection de la sphere
                            Vector3 randVect = new Vector3((float)random.NextDouble(), (float)random.NextDouble(), (float)random.NextDouble());
                            Vector3 xBase = Vector3.Cross(normal, randVect);
                            Vector3 yBase = Vector3.Cross(normal, xBase);
                            Vector3 zBase = normal;

                            //calcul d'une nouvelle direction grace à la nouvelle base
                            Vector3 newDirect = Vector3.Add(Vector3.Add(xBase * mini_sphere_coord.X, yBase * mini_sphere_coord.Y), zBase * mini_sphere_coord.Z);
                            Vector3 normalNewDirect = Vector3.Normalize(newDirect);


                            //dans la generation de la direction aléatoire du rebond, pour ne pas avoir de vecteur qui va dans la sphere
                            if (Vector3.Dot(normalNewDirect, normal) < 0)
                            {
                                newDirect = -newDirect;
                            }

                            //création d'un nouveau rayon faire le prochain rebond
                            Ray rebond = new Ray(interPoint, newDirect);
                            //cumul des albedo de toutes les surfaces sur lesquelles on rebondit
                            color = Vector3.Add(lightPower, Vector3.Multiply(Vector3.Dot(normalNewDirect, normal), Vector3.Add(resInter.sph.material.albedo, Radiance(rebond, ++stop, arbre))));


                            //division du cumul d'albedo par le nombre de rebond
                            color = Vector3.Divide(color, nb_rebond);

                            break;


                        //MIRROIR
                        case MaterialType.Mirror:
                            //calcul de la nouvelle direction
                            Vector3 newDir = Vector3.Add(Vector3.Multiply(2 * -Vector3.Dot(ray.direction, normal), normal), ray.direction);
                            Ray reflection = new Ray(interPoint, newDir);
                            color = Vector3.Multiply(resInter.sph.material.albedo, Radiance(reflection, ++stop, arbre));
                            break;

                        case MaterialType.Light:
                            color = resInter.sph.material.albedo;
                            break;
                    }
                }
            }
            return color;
        }

        //res_intersect structure contenant la distance t à la sphere et la sphere sph
        public struct ResIntersect
        {
            public float t;
            public Sphere sph;
        }

        //retourne la structure res qui contient la distance et la sphere pour laquelle on a une intersection
        public ResIntersect Intersects(Ray r)
        {
            ResIntersect res;
            res.t = -1;
            res.sph = null;
            foreach (Sphere s in spheres)
            {
                float inter = Intersect(r, s);
                if (inter != -1)
                {
                    if (res.t == -1 || inter < res.t)
                    {
                        res.t = inter;
                        res.sph = s;
                    }
                }
            }
            return res;
        }

        //fonction d'intersection avec une sphere
        public float Intersect(Ray r, Sphere s)
        {
            float A = Vector3.Dot(r.direction, r.direction);
            float B = 2 * (Vector3.Dot(r.point, r.direction) - Vector3.Dot(s.center, r.direction));
            float C = Vector3.Dot(Vector3.Subtract(s.center, r.point), Vector3.Subtract(s.center, r.point)) - (s.radius * s.radius);
            float D = B * B - 4 * A * C;
            if (D < 0)
                return -1.0f;
            else
            {
                float i1 = ((-B) + (float)Math.Sqrt(D)) / (2 * A);
                float i2 = ((-B) - (float)Math.Sqrt(D)) / (2 * A);
                if (i2 > 0)
                    return i2;
                else if (i1 > 0)
                    return i1;
                else
                    return -1.0f;
            }
        }

        //fonction de diffusion
        public Vector3 ReceiveLight(Light light, Vector3 interPoint, Sphere sphere)
        {
            Vector3 l = Vector3.Subtract(light.origin, interPoint);
            float dist = l.Length();
            l = Vector3.Normalize(l);
            Vector3 n = Vector3.Subtract(interPoint, sphere.center);
            n = Vector3.Normalize(n);

            Vector3 powerReceived = Vector3.Multiply(light.power, 1 / (dist * dist));
            Vector3 lightEmmited = Vector3.Divide(Vector3.Multiply(sphere.material.albedo, Clamp(Vector3.Dot(n, l), 0.0f, 1.0f)), (float)Math.PI);

            return Vector3.Multiply(powerReceived, lightEmmited);
        }

        public float Clamp(float v, float min, float max)
        {
            return Math.Max(Math.Min(v, max), min);
        }

        //renvoie un point aléatoire sur la sphere pour avoir un vecteur aléatoire
        public Vector3 RandomVector(Vector3 pointSphere)
        {
            double n1 = random.NextDouble();
            double n2 = random.NextDouble();

            double x = Math.Cos(2 * Math.PI * n1) * Math.Sqrt(1 - n2);
            double y = Math.Sin(2 * Math.PI * n1) * Math.Sqrt(1 - n2);
            double z = Math.Sqrt(n2);

            Vector3 coordonnees = new Vector3((float)x, (float)y, (float)z);
            return coordonnees;
        }




        //PARTIE INTERSECT RAY BOX

        public bool RayBoxIntersect(Ray pos, Box B)
        {
            Vector3 normalizedDir = pos.direction;
            float rinvx = 1 / normalizedDir.X;
            float rinvy = 1 / normalizedDir.Y;
            float rinvz = 1 / normalizedDir.Z;

            //X slab
            float tx1 = (B.pointMin.X - pos.point.X) * rinvx;
            float tx2 = (B.pointMax.X - pos.point.X) * rinvx;

            float tminX = Math.Min(tx1, tx2);
            float tmaxX = Math.Max(tx1, tx2);

            //Y slab
            float ty1 = (B.pointMin.Y - pos.point.Y) * rinvy;
            float ty2 = (B.pointMax.Y - pos.point.Y) * rinvy;

            float tminY = Math.Max(tminX, Math.Min(ty1, ty2));
            float tmaxY = Math.Min(tmaxX, Math.Max(ty1, ty2));

            //Z slab
            float tz1 = (B.pointMin.Z - pos.point.Z) * rinvz;
            float tz2 = (B.pointMax.Z - pos.point.Z) * rinvz;

            float tminZ = Math.Max(tminY, Math.Min(tz1, tz2));
            float tmaxZ = Math.Min(tmaxY, Math.Max(tz1, tz2));


            return tmaxZ >= tminZ;
        }

        //union de boites
        public Box BoxUnion(Box B1, Box B2)
        {
            float xMin = Math.Min(B1.pointMin.X, B2.pointMin.X);
            float yMin = Math.Min(B1.pointMin.Y, B2.pointMin.Y);
            float zMin = Math.Min(B1.pointMin.Z, B2.pointMin.Z);

            float xMax = Math.Max(B1.pointMax.X, B2.pointMax.X);
            float yMax = Math.Max(B1.pointMax.Y, B2.pointMax.Y);
            float zMax = Math.Max(B1.pointMax.Z, B2.pointMax.Z);
            Vector3 min = new Vector3(xMin, yMin, zMin);
            Vector3 max = new Vector3(xMax, yMax, zMax);

            Box Union = new Box(min, max, null);
            return Union;
        }


        public Box BoxesUnion(List<Box> liste)
        {
            Box tmp = liste[0];
            for (int i = 1; i < liste.Count; i++)
            {
                tmp = BoxUnion(tmp, liste[i]);
            }
            return tmp;
        }


        //Tri de la liste/tableau de box
        public List<Box> TriBox(List<Box> listeBox)
        {
            List<Box> BoxTri = listeBox.OrderBy(o => o.pointMin.X).ToList();
            return BoxTri;
        }


        public Tree ConstructTree(List<Box> Liste)
        {
            Tree Noeud;
            if (Liste.Count == 1)
            {
                Noeud = new Tree(true, Liste[0], null, null, Liste[0].sphere);
                return Noeud;
            }

            Box boiteEnglobante = BoxesUnion(Liste);
            //tri la liste de boite selon l'axe X
            List<Box> BoiteTriee = TriBox(Liste);
            List<Box> TreeRight = new List<Box>();
            List<Box> TreeLeft = new List<Box>();
            //sépare la liste en 2
            for (int i = 0; i < BoiteTriee.Count; i++)
            {
                if (i < BoiteTriee.Count / 2)
                {
                    TreeLeft.Add(BoiteTriee[i]);
                }
                else
                {
                    TreeRight.Add(BoiteTriee[i]);
                }
            }
            Noeud = new Tree(false, boiteEnglobante, ConstructTree(TreeRight), ConstructTree(TreeLeft), null);
            return Noeud;
        }


        public ResIntersect IntersectTree(Tree arbre, Ray pos)
        {
            ResIntersect a;
            a.sph = null;
            a.t = -1;

            if (arbre.isLeaf)
            {
                float inter = Intersect(pos, arbre.sphere);
                if (inter != -1)
                {
                    if (a.t == -1 || inter < a.t)
                    {
                        a.t = inter;
                        a.sph = arbre.sphere;
                    }
                }
                return a;
            }

            if (RayBoxIntersect(pos, arbre.boite))
            {
                a = IntersectTree(arbre.TreeA, pos);
                if (a.t == -1)
                {
                    a = IntersectTree(arbre.TreeB, pos);
                }

            }
            return a;
        }
    }
}
