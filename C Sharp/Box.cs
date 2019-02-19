using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace Synthese_Image
{
    class Box
    {
        public Vector3 pointMax;
        public Vector3 pointMin;
        public Sphere sphere;
        public Box(Vector3 c, float r, Sphere s)
        {
            pointMax = new Vector3(c.X + r, c.Y + r, c.Z + r);
            pointMin = new Vector3(c.X - r, c.Y - r, c.Z - r);
            sphere = s;
        }
        public Box(Vector3 pMin, Vector3 pMax, Sphere s)
        {
            pointMin = pMin;
            pointMax = pMax;
            sphere = s;
        }
    }
}
