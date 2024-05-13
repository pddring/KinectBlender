using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
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
            s.WriteLine(" \"valid\": true,");
            s.WriteLine(" \"lean\": {");
            s.WriteLine($"  \"x\":{b.Lean.X},");
            s.WriteLine($"  \"y\":{b.Lean.Y}");
            s.WriteLine(" },");

            s.WriteLine(" \"joints\": [");
            bool firstJoint = true;
            foreach (JointType j in b.Joints.Keys)
            {
                if (firstJoint)
                {
                    firstJoint = false;
                } else
                {
                    s.WriteLine(",");
                }
                s.WriteLine("  {");
                s.WriteLine($"   \"name\":\"{j}\",");               

                CameraSpacePoint pos = b.Joints[j].Position;
                if (pos.Z < 0)
                {
                    pos.Z = InferredZPositionClamp;
                }
                mapper.MapCameraPointToDepthSpace(pos);
                Vector4 quat = b.JointOrientations[j].Orientation;

                s.WriteLine($"   \"x\":{pos.X},");
                s.WriteLine($"   \"y\":{pos.Y},");
                s.WriteLine($"   \"z\":{pos.Z}");
                s.WriteLine($"   \"orientation\": [{quat.W}, {quat.X}, {quat.Y}, {quat.Z}]");
                s.Write("  }");
            }

            s.WriteLine(" ]");
            s.Write("}");
            return s.ToString();
        }

        public override string ToString()
        {
            return GetJSON();
        }
    }
}
