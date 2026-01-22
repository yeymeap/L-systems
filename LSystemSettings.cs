using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace L_systems
{
    public class LSystemSettings
    {
        public string Axiom { get; set; }
        public string Rules { get; set; }
        public string Variables { get; set; }
        public string Constants { get; set; }
        public int Iterations { get; set; }
        public float Angle { get; set; }
        public float Step { get; set; }
        public float PenWidth { get; set; }
        public int CanvasColor { get; set; }
        public int PenColor { get; set; }
    }
}
