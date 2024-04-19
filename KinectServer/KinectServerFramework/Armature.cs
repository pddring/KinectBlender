using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinectServerFramework
{
    public class Armature
    {
        public int id;
        public Body b;
        private const float InferredZPositionClamp = 0.1f;
        CoordinateMapper mapper;
        public Armature(Body b, int id, CoordinateMapper mapper)
        {
            this.id = id;
            this.b = b;
            this.mapper = mapper;
        }

        public string GetJSON()
        {
            StringWriter s = new StringWriter();
            s.WriteLine("{");
            s.WriteLine(" \"valid\": \"true\",");
            s.WriteLine(" \"lean\": {");
            s.WriteLine($"  \"x\":\"{b.Lean.X}\",");
            s.WriteLine($"  \"y\":\"{b.Lean.Y}\"");
            s.WriteLine(" \"},");
            s.WriteLine(" \"joints\": [");
            bool firstJoint = true;
            foreach (JointType j in b.Joints.Keys)
            {
                if (firstJoint)
                {
                    s.Write(",");
                    firstJoint = false;
                }
                s.WriteLine("  {");
                s.WriteLine($"   \"name\":\"{j}\",");

                CameraSpacePoint pos = b.Joints[j].Position;
                if (pos.Z < 0)
                {
                    pos.Z = InferredZPositionClamp;
                }
                mapper.MapCameraPointToDepthSpace(pos);

                s.WriteLine($"   \"x\":\"{pos.X}\",");
                s.WriteLine($"   \"y\":\"{pos.Y}\",");
                s.WriteLine($"   \"z\":\"{pos.Z}\",");
                s.WriteLine("  }");
            }

            s.WriteLine(" ]");
            s.WriteLine("}");
            return s.ToString();
        }

        public override string ToString()
        {
            return GetJSON();
            
        }
    }
}
