using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.Structure;

namespace Emotion_Detection
{
    class FaceData
    {
    public int Emotion {get; set;}

    public List<Image<Gray, byte>> Images{get; set; }
 }
}
