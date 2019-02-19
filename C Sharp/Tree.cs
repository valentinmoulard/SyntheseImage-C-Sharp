using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Synthese_Image
{
    class Tree
    {
        public bool isLeaf;
        public Box boite;
        public Tree TreeA;
        public Tree TreeB;
        public Sphere sphere;

        public Tree(bool leaf, Box c, Tree A, Tree B, Sphere s)
        {
            isLeaf = leaf;
            boite = c;
            TreeA = A;
            TreeB = B;
            sphere = s;
        }
    }
}
