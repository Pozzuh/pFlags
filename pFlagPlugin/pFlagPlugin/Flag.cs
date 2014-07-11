using System;
using System.Collections.Generic;
using System.Text;
using Addon;

namespace pFlagPlugin
{
    class Flag
    {
        private bool _bothWays = false;
        private Vector _fIn;
        private Vector _fOut;
        private Entity _entityIn;
        private Entity _entityOut;

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
    }
}
