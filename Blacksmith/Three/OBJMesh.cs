using OpenTK;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

// adapted from: http://neokabuto.blogspot.com/2015/04/opentk-tutorial-7-simple-objects-from.html

namespace Blacksmith.Three
{
    public class OBJMesh : Mesh
    {
        private Vector3[] vertices;
        private Vector3[] colors;
        private Vector2[] texturecoords;
        private Vector3[] normals;

        private List<Tuple<int, int, int>> faces = new List<Tuple<int, int, int>>();
        
        public override Vector3[] GetVertices() => vertices;

        public override int[] GetIndices(int offset = 0)
        {
            List<int> temp = new List<int>();

            foreach (var face in faces)
            {
                temp.Add(face.Item1 + offset);
                temp.Add(face.Item2 + offset);
                temp.Add(face.Item3 + offset);
            }

            return temp.ToArray();
        }
        
        public override Vector3[] GetColorData() => colors;
        
        public override void CalculateModelMatrix()
        {
            ModelMatrix = Matrix4.CreateScale(Scale) * Matrix4.CreateRotationX(Rotation.X) * Matrix4.CreateRotationY(Rotation.Y) * Matrix4.CreateRotationZ(Rotation.Z) * Matrix4.CreateTranslation(Position);
        }

        public override Vector2[] GetTextureCoords()
        {
            return new[] { Vector2.Zero };
        }

        public override Vector3[] GetNormals() => normals;

        public void CalculateNormals()
        {
            normals = new Vector3[GetVertices().Length];
            Vector3[] verts = GetVertices();
            int[] inds = GetIndices();

            // Compute normals for each face
            for (int i = 0; i < GetIndices().Length; i += 3)
            {
                Vector3 v1 = verts[inds[i]];
                Vector3 v2 = verts[inds[i + 1]];
                Vector3 v3 = verts[inds[i + 2]];

                // The normal is the cross product of two sides of the triangle
                normals[inds[i]] += Vector3.Cross(v2 - v1, v3 - v1);
                normals[inds[i + 1]] += Vector3.Cross(v2 - v1, v3 - v1);
                normals[inds[i + 2]] += Vector3.Cross(v2 - v1, v3 - v1);
            }

            for (int i = 0; i < NormalCount; i++)
            {
                normals[i] = normals[i].Normalized();
            }
        }
        
        //

        public static OBJMesh LoadFromFile(string filename)
        {
            OBJMesh obj = new OBJMesh();
            try
            {
                using (StreamReader reader = new StreamReader(new FileStream(filename, FileMode.Open, FileAccess.Read)))
                {
                    obj = LoadFromString(reader.ReadToEnd());
                }
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("File not found: {0}", filename);
            }
            catch (Exception)
            {
                Console.WriteLine("Error loading file: {0}", filename);
            }

            return obj;
        }

        public static OBJMesh LoadFromString(string obj)
        {
            // Separate lines from the file
            List<string> lines = new List<string>(obj.Split('\n'));

            // Lists to hold model data
            List<Vector3> verts = new List<Vector3>();
            List<Vector3> colors = new List<Vector3>();
            List<Vector2> texs = new List<Vector2>();
            List<Tuple<int, int, int>> faces = new List<Tuple<int, int, int>>();

            // Read file line by line
            foreach (string line in lines)
            {
                if (line.StartsWith("v ")) // Vertex definition
                {
                    // Cut off beginning of line
                    string temp = line.Substring(2);

                    Vector3 vec = new Vector3();

                    if (temp.Count((char c) => c == ' ') == 2) // Check if there's enough elements for a vertex
                    {
                        string[] vertparts = temp.Split(' ');

                        // Attempt to parse each part of the vertice
                        bool success = float.TryParse(vertparts[0], out vec.X);
                        success &= float.TryParse(vertparts[1], out vec.Y);
                        success &= float.TryParse(vertparts[2], out vec.Z);

                        // Dummy color/texture coordinates for now
                        colors.Add(new Vector3((float)Math.Sin(vec.Z), (float)Math.Sin(vec.Z), (float)Math.Sin(vec.Z)));
                        texs.Add(new Vector2((float)Math.Sin(vec.Z), (float)Math.Sin(vec.Z)));

                        // If any of the parses failed, report the error
                        if (!success)
                        {
                            Console.WriteLine("Error parsing vertex: {0}", line);
                        }
                    }

                    verts.Add(vec);
                }
                else if (line.StartsWith("f ")) // Face definition
                {
                    // Cut off beginning of line
                    string temp = line.Substring(2);

                    Tuple<int, int, int> face = new Tuple<int, int, int>(0, 0, 0);

                    if (temp.Count((char c) => c == ' ') == 2) // Check if there's enough elements for a face
                    {
                        string[] faceparts = temp.Split(' ');

                        // Attempt to parse each part of the face
                        bool success = int.TryParse(faceparts[0], out int i1);
                        success &= int.TryParse(faceparts[1], out int i2);
                        success &= int.TryParse(faceparts[2], out int i3);

                        // If any of the parses failed, report the error
                        if (!success)
                        {
                            Console.WriteLine("Error parsing face: {0}", line);
                        }
                        else
                        {
                            // Decrement to get zero-based vertex numbers
                            face = new Tuple<int, int, int>(i1 - 1, i2 - 1, i3 - 1);
                            faces.Add(face);
                        }
                    }
                }
            }
            
            return new OBJMesh
            {
                vertices = verts.ToArray(),
                faces = new List<Tuple<int, int, int>>(faces),
                colors = colors.ToArray(),
                texturecoords = texs.ToArray()
            };
        }
    }
}