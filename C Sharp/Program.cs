using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Numerics;

namespace Synthese_Image
{
    class Program
    {
        static void Main(string[] args)
        {
            //===============SCENE=1=============================================================================================================================

            Sphere light = new Sphere(new Vector3(640, 360, 500), 10, new Material(MaterialType.Light, new Vector3(1, 1, 1)));

            //murs
            Sphere leftWall = new Sphere(new Vector3((float)-1e5, 360, 500), (float)1e5, new Material(MaterialType.Difuse, new Vector3(1, 0.5f, 0.1f)));
            Sphere rightWall = new Sphere(new Vector3((float)1e5 + 1280, 360, 500), (float)1e5, new Material(MaterialType.Difuse, new Vector3(0.1f, 1, 0.5f)));
            Sphere topWall = new Sphere(new Vector3(640, (float)-1e5, 500), (float)1e5, new Material(MaterialType.Difuse, new Vector3(0.1f, 0.5f, 1)));
            Sphere bottomWall = new Sphere(new Vector3(640, (float)1e5 + 720, 500), (float)1e5, new Material(MaterialType.Difuse, new Vector3(1, 1, 0.1f)));
            Sphere backWall = new Sphere(new Vector3(640, 360, (float)1e5 + 1000), (float)1e5, new Material(MaterialType.Difuse, new Vector3(0.5f, 0.1f, 1)));
            Sphere frontWall = new Sphere(new Vector3(640, 360, (float)-1e5), (float)1e5, new Material(MaterialType.Difuse, new Vector3(0, 0.1f, 1)));


            Sphere sLeft = new Sphere(new Vector3(100, 600, 500), 100, new Material(MaterialType.Difuse, new Vector3(0.1f, 1, 0.1f)));
            Sphere sLeftDown = new Sphere(new Vector3(370, 490, 300), 125, new Material(MaterialType.Mirror, new Vector3(1, 1, 0.1f)));
            Sphere sDown = new Sphere(new Vector3(640, 620, 500), 100, new Material(MaterialType.Difuse, new Vector3(0.1f, 1, 1)));
            Sphere sRightDown = new Sphere(new Vector3(910, 490, 700), 100, new Material(MaterialType.Mirror, new Vector3(0.5f, 1, 0.5f)));
            Sphere sRight = new Sphere(new Vector3(1180, 360, 900), 100, new Material(MaterialType.Difuse, new Vector3(1, 0.1f, 0.5f)));


            Box Obox = new Box(new Vector3(640, 620, 500), 100, sDown);
            Box Obox1 = new Box(new Vector3(100, 600, 500), 100, sLeft);
            Box Obox2 = new Box(new Vector3(1180, 360, 900), 100, sRight);
            Box Obox3 = new Box(new Vector3(370, 490, 300), 125, sLeftDown);
            Box Obox4 = new Box(new Vector3(910, 490, 700), 100, sRightDown);
            Box Obox5 = new Box(new Vector3(640, 360, 500), 10, light);
            //===============SCENE=2=============================================================================================================================

            Sphere light2 = new Sphere(new Vector3(640, 50, 500), 10, new Material(MaterialType.Light, new Vector3(1, 1, 1)));

            Sphere leftWall2 = new Sphere(new Vector3((float)-1e5, 360, 500), (float)1e5, new Material(MaterialType.Mirror, new Vector3(0.9f, 0.9f, 0.9f)));
            Sphere rightWall2 = new Sphere(new Vector3((float)1e5 + 1280, 360, 500), (float)1e5, new Material(MaterialType.Mirror, new Vector3(0.9f, 0.9f, 0.9f)));
            Sphere topWall2 = new Sphere(new Vector3(640, (float)-1e5, 500), (float)1e5, new Material(MaterialType.Mirror, new Vector3(0.9f, 0.9f, 0.9f)));
            Sphere bottomWall2 = new Sphere(new Vector3(640, (float)1e5 + 720, 500), (float)1e5, new Material(MaterialType.Mirror, new Vector3(0.9f, 0.9f, 0.9f)));
            Sphere backWall2 = new Sphere(new Vector3(640, 360, (float)1e5 + 1000), (float)1e5, new Material(MaterialType.Mirror, new Vector3(0.9f, 0.9f, 0.9f)));
            Sphere frontWall2 = new Sphere(new Vector3(640, 360, (float)-1e5), (float)1e5, new Material(MaterialType.Mirror, new Vector3(0.9f, 0.9f, 0.9f)));

            Sphere sph = new Sphere(new Vector3(640, 620, 500), 100, new Material(MaterialType.Difuse, new Vector3(1, 1, 1)));
            //===================================================================================================================================================

            Sphere[] sphL = { light, leftWall, rightWall, topWall, bottomWall, backWall, frontWall, sDown, sLeft, sRight, sLeftDown, sRightDown };
            Light lgt = new Light(new Vector3(640, 360, 500), new Vector3(1000000, 1000000, 1000000));

            Sphere[] sphL2 = { light2, leftWall2, rightWall2, topWall2, bottomWall2, backWall2, frontWall2, sph };
            Light lgt2 = new Light(new Vector3(640, 50, 500), new Vector3(1000000, 1000000, 1000000));

            Camera cam = new Camera(new Vector3(0, 0, 0), 1280, 720, new Vector3(640, 360, -1000));


            int rangeX = 1280;
            int rangeY = 720;
            int rangeZ = 1000;
            int rangeR = 100;
            Random r = new Random();

            List<Box> ListBox = new List<Box>();
            //pour les boites
            for (int i = 0; i < 200; i++)
            {
                float rayon = (float)r.NextDouble() * rangeR;
                Vector3 position = new Vector3((float)r.NextDouble() * rangeX, (float)r.NextDouble() * rangeY, (float)r.NextDouble() * rangeZ);
                ListBox.Add(new Box(position, rayon, new Sphere(position, rayon, new Material(MaterialType.Difuse, new Vector3((float)r.NextDouble(), (float)r.NextDouble(), (float)r.NextDouble())))));
            }

            //List<Box> ListBox = new List<Box> { Obox1, Obox2, Obox, Obox3, Obox4, Obox5 };
            Scene testBoite = new Scene("testBoite", sphL, cam, lgt, ListBox);



            /*
            //pour les spheres
			Scene scn = new Scene("Scene", sphL, cam, lgt);
			Scene scn2 = new Scene("MirrorCeption", sphL2, cam, lgt2);
            */

            testBoite.DrawScene();

            /*scn2.DrawScene();*/









            /*test Tri Box
            List<Box> resultat = testBoite.TriBox(ListBox);
            Console.WriteLine(resultat.Count);
            for (int i = 0; i < resultat.Count; i++)
            {
                Console.WriteLine(resultat[i].pointMin);
                Console.WriteLine(resultat[i].pointMax);
                Console.WriteLine("===============");
                Thread.Sleep(10000);
            }
            */

            /*test UnionBox
            Box resultat = testBoite.BoxesUnion(ListBox);
            Console.WriteLine(resultat.pointMin);
            Console.WriteLine(resultat.pointMax);
            */
        }
    }
}
