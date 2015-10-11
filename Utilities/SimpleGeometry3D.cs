using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Utilities
{
    public class SimpleGeometry3D
    {
        public static MeshGeometry3D CreateSphere(Point3D center, double radius, int slices, int angularSlices)
        {
            var vertices = new Point3DCollection();
            var normals = new Vector3DCollection();
            var facets = new Int32Collection();

            var angularIncrease = 2 * Math.PI / angularSlices;
            var heightIncrease = 2 * radius / (slices-1);

            for (int i = 1; i < slices - 1; i++)
            {
                var height = i * heightIncrease - radius;
                var layerRadius = Math.Sqrt(radius * radius - Math.Abs(height) * Math.Abs(height));

                for (int j = 0; j < angularSlices; j++)
                { 
                    var angle = angularIncrease * j;
                    var xCoord = Math.Cos(angle) * layerRadius;
                    var yCoord = Math.Sin(angle) * layerRadius;
                    var cicleVector = new Vector3D(xCoord, yCoord, height);
                    vertices.Add(center + cicleVector);
                    normals.Add(cicleVector);

                    if (i != slices - 2)
                    {
                        var index = (i-1)*angularSlices + j;
                        var index2 = (i-1)*angularSlices + (j+1)%angularSlices;
                        var index3 = (i)*angularSlices + j;
                        var index4 = (i)*angularSlices + (j+1)%angularSlices;
                        facets.Add(index);
                        facets.Add(index2);
                        facets.Add(index3);
                        facets.Add(index4);
                        facets.Add(index3);
                        facets.Add(index2);
                    }
                }
            }

            vertices.Add(center + new Vector3D(0, 0, radius));
            normals.Add(new Vector3D(0, 0, 1));

            for (int i = 0; i < angularSlices; i++)
            {
                var index = (slices - 2) * angularSlices;
                var index2 = (slices - 3) * angularSlices + i;
                var index3 = (slices - 3) * angularSlices + (i + 1) % angularSlices;

                facets.Add(index);
                facets.Add(index2);
                facets.Add(index3);
            }

            vertices.Add(center - new Vector3D(0, 0, radius));
            normals.Add(new Vector3D(0, 0, -1));

            for (int i = 0; i < angularSlices; i++)
            {
                var index = (slices - 2) * angularSlices + 1;
                var index2 = i;
                var index3 = (i + 1) % angularSlices;

                facets.Add(index3);
                facets.Add(index2);
                facets.Add(index);
            }

            var sphere = new MeshGeometry3D();
            sphere.Positions = vertices;
            sphere.Normals = normals;
            sphere.TriangleIndices = facets;

            return sphere;
        }

        public static MeshGeometry3D CreateRectangle(double height, double width)
        {
            var vertices = new Point3DCollection();
            var normals = new Vector3DCollection();
            var facets = new Int32Collection();
            var textureCoords = new PointCollection();
            vertices.Add(new Point3D(-width / 2, -height / 2, 0));
            vertices.Add(new Point3D(-width / 2, height / 2, 0));
            vertices.Add(new Point3D(width / 2, -height / 2, 0));
            vertices.Add(new Point3D(width / 2, height / 2, 0));

            normals.Add(new Vector3D(0, 0, 1));
            normals.Add(new Vector3D(0, 0, 1));
            normals.Add(new Vector3D(0, 0, 1));
            normals.Add(new Vector3D(0, 0, 1));

            textureCoords.Add(new Point(0,0));
            textureCoords.Add(new Point(0, 1));
            textureCoords.Add(new Point(1,0));
            textureCoords.Add(new Point(1,1));

            facets.Add(0); facets.Add(1); facets.Add(2); facets.Add(3); facets.Add(2); facets.Add(1);
            var rectangle = new MeshGeometry3D();
            rectangle.Positions = vertices;
            rectangle.Normals = normals;
            rectangle.TriangleIndices = facets;
            rectangle.TextureCoordinates = textureCoords;
            return rectangle;
        }
    }
}
