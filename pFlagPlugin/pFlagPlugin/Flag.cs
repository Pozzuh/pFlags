using System;
using System.Collections.Generic;
using System.Text;
using Addon;
//this shit is a mess, I know
namespace pFlagPlugin
{
    class Flag
    {
        private Vector _fIn;
        private Vector _fOut;

        public Flag(Vector v1, Vector v2)
        {
            _fIn = v1;
            _fOut = v2;
        }

        public Flag(int x1, int y1, int z1, int x2, int y2, int z2)
        {
            _fIn = new Vector(x1, y1, z1);
            _fOut = new Vector(x2, y2, z2);
        }

        public bool bothWays { get; set; }
        public Entity entityIn { get; set; }
        public Entity entityOut { get; set; }

        public Vector positionIn
        {
            get { return _fIn; }          
        }

        public Vector positionOut
        {
            get { return _fOut; }
        }

        public string modelIn { get; set; }
        public string modelOut { get; set; }
        public bool hideOut { get; set; }
        public bool autoUseIn { get; set; }
    }
}
