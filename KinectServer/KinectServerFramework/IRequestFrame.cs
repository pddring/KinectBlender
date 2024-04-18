using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinectServerFramework
{
    internal interface IRequestFrame
    {
        /// <summary>
        /// Gets a JSON representation of an armature defined by joint positions
        /// </summary>
        /// <param name="bodyID">0-5 index</param>
        /// <returns>JSON</returns>
        string GetArmature(int bodyID);
    }
}
