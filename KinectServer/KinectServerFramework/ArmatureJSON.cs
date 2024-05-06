
using Microsoft.Kinect;
using System.IO;

public class ArmatureJSON
{
    public bool valid { get; set; }
    public Lean lean { get; set; }
    public Joint[] joints { get; set; }

    public string GetJSON()
    {
        StringWriter s = new StringWriter();
        s.WriteLine("{");
        s.WriteLine(" \"valid\": true,");
        s.WriteLine(" \"lean\": {");
        s.WriteLine($"  \"x\":{lean.x},");
        s.WriteLine($"  \"y\":{lean.y}");
        s.WriteLine(" },");
        s.WriteLine(" \"joints\": [");
        bool firstJoint = true;
        foreach (Joint j in joints)
        {
            if (firstJoint)
            {
                firstJoint = false;
            }
            else
            {
                s.WriteLine(",");
            }
            s.WriteLine("  {");
            s.WriteLine($"   \"name\":\"{j.name}\",");

            s.WriteLine($"   \"x\":{j.x},");
            s.WriteLine($"   \"y\":{j.y},");
            s.WriteLine($"   \"z\":{j.z}");
            s.Write("  }");
        }

        s.WriteLine(" ]");
        s.Write("}");
        return s.ToString();
    }
}

public class Lean
{
    public float x { get; set; }
    public float y { get; set; }
}

public class Joint
{
    public string name { get; set; }
    public float x { get; set; }
    public float y { get; set; }
    public float z { get; set; }
}
