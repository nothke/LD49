using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IFL.Rendering.Water
{
    public class ReflectionCamera : MonoBehaviour
    {
        private void OnPreRender()
        {
            GL.invertCulling = true;
        }

        private void OnPostRender()
        {
            GL.invertCulling = false;
        }
    }
}