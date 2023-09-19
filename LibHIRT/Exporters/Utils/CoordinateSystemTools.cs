using Aspose.ThreeD;
using Aspose.ThreeD.Entities;
using Aspose.ThreeD.Utilities;

namespace LibHIRT.Exporters.Utils
{
    public static class CoordinateSystemTools
    {
        public static void ChangeCoordenate(Node rootNode, bool nz = false)
        {
            rootNode.Accept((node) =>
            {
                var globalTransform = node.EvaluateGlobalTransform(false);
                if (node.Name.Contains("instance_")) {
                    bool found = false;
                    if (node.Transform.EulerAngles.x > 180 || node.Transform.EulerAngles.x < -90) {
                        node.SetProperty("bug X", node.Transform.EulerAngles.x);
                        found = true;
                    }
                    if (node.Transform.EulerAngles.y > 180 || node.Transform.EulerAngles.y < -90) {
                        node.SetProperty("bug Y", node.Transform.EulerAngles.y);
                        found = true;
                    }
                    if (node.Transform.EulerAngles.z > 180 || node.Transform.EulerAngles.z < -90) {
                        node.SetProperty("bug Z", node.Transform.EulerAngles.z);
                        found = true;
                    }
                    if (found) {
                        node.SetProperty("xyz position", node.Transform.Translation);

                        Vector3 temp = new Vector3(node.Transform.Rotation.x, node.Transform.Rotation.y, node.Transform.Rotation.z);
                        node.SetProperty("rot quater W", node.Transform.Rotation.w);
                        node.SetProperty("rot quater xyz", temp);
                        node.SetProperty("rot quater str", node.Transform.Rotation.ToString());
                        node.SetProperty("xyz scale", node.Transform.Scale);
                        
                    }
                    node.SetProperty("bug", found);
                    node.SetProperty("xyz angles", node.Transform.EulerAngles);
                    //node.Transform.EulerAngles = node.Transform.EulerAngles.ToPositiveAngle();

                }
                return true;
            });
        }

        private static Vector3 radiansToAngle(Vector3 vector) {
            return new Vector3(
                     vector.x * (180 / Math.PI),
                     vector.y * (180 / Math.PI),
                     vector.z * (180 / Math.PI)
                );
        }

        public static void ChangeCoordenateOnMesh(Node rootNode, bool nz = false)
        {
            rootNode.Accept((node) =>
            {
                var globalTransform = node.EvaluateGlobalTransform(false);
                foreach (var entity in node.Entities)
                {
                    if (entity is Geometry geom)
                    {
                        for (int i = 0; i < geom.ControlPoints.Count; i++)
                        {
                            geom.ControlPoints[i] = globalTransform * geom.ControlPoints[i];
                        }
                    }
                }
                return true;
            });
        }
    }
}
