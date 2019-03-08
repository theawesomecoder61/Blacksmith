using OpenTK;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

// adapted from: https://github.com/neokabuto/OpenTKTutorialContent/blob/master/OpenTKTutorial9-3/OpenTKTutorial9-3/ObjVolume.cs

namespace Blacksmith.Three
{
    public class OBJModel : Model
    {
        class FaceVertex
        {
            public Vector3 Position;
            public Vector3 Normal;
            public Vector2 TextureCoord;

            public FaceVertex(Vector3 pos, Vector3 norm, Vector2 texcoord)
            {
                Position = pos;
                Normal = norm;
                TextureCoord = texcoord;
            }
        }

        class TempVertex
        {
            public int Vertex;
            public int Normal;
            public int Texcoord;

            public TempVertex(int vert = 0, int norm = 0, int tex = 0)
            {
                Vertex = vert;
                Normal = norm;
                Texcoord = tex;
            }
        }

        private List<Tuple<FaceVertex, FaceVertex, FaceVertex>> faces = new List<Tuple<FaceVertex, FaceVertex, FaceVertex>>();

        public override int VertexCount { get { return faces.Count * 3; } }
        public override int FaceCount { get { return faces.Count; } }
        public override int IndexCount { get { return faces.Count * 3; } }
        public override int ColorDataCount { get { return faces.Count * 3; } }
        public override int TextureCoordsCount { get { return faces.Count * 3; } }

        public override Vector3[] GetVertices()
        {
            List<Vector3> verts = new List<Vector3>();

            foreach (var face in faces)
            {
                verts.Add(face.Item1.Position);
                verts.Add(face.Item2.Position);
                verts.Add(face.Item3.Position);
            }

            return verts.ToArray();
        }

        public override int[] GetIndices(int offset = 0) => Enumerable.Range(offset, IndexCount).ToArray();
        
        public override Vector3[] GetColorData() => new Vector3[ColorDataCount];
        
        public override void CalculateModelMatrix()
        {
            ModelMatrix = Matrix4.CreateScale(Scale) * Matrix4.CreateRotationX(Rotation.X) * Matrix4.CreateRotationY(Rotation.Y) * Matrix4.CreateRotationZ(Rotation.Z) * Matrix4.CreateTranslation(Position);
        }

        public override Vector2[] GetTextureCoords()
        {
            List<Vector2> coords = new List<Vector2>();

            foreach (var face in faces)
            {
                coords.Add(face.Item1.TextureCoord);
                coords.Add(face.Item2.TextureCoord);
                coords.Add(face.Item3.TextureCoord);
            }

            return coords.ToArray();
        }

        public override Vector3[] GetNormals()
        {
            if (base.GetNormals().Length > 0)
            {
                return base.GetNormals();
            }

            List<Vector3> normals = new List<Vector3>();

            foreach (var face in faces)
            {
                normals.Add(face.Item1.Normal);
                normals.Add(face.Item2.Normal);
                normals.Add(face.Item3.Normal);
            }

            return normals.ToArray();
        }

        public override Vector3[] CalculateAABB()
        {
            float minX = GetVertices().Select(x => x.X).Min() * Scale.X;
            float maxX = GetVertices().Select(x => x.X).Max() * Scale.X;
            float minY = GetVertices().Select(x => x.Y).Min() * Scale.X;
            float maxY = GetVertices().Select(x => x.Y).Max() * Scale.X;
            float minZ = GetVertices().Select(x => x.Z).Min() * Scale.X;
            float maxZ = GetVertices().Select(x => x.Z).Max() * Scale.X;
            /* Illustration of order of Vector3s
             *   E __________________ F
             *    /|               /|
             *   / |              / |
             * A ----------------- B|
             *   | |             |  |
             *   | |G____________|__| H         ^
             *   | /             | /           / Z+
             * C |/______________|/ D   X- <--/
             */
            return new Vector3[]
            {
                new Vector3(minX, maxY, minZ), // A
                new Vector3(maxX, maxY, minZ), // B
                new Vector3(minX, minY, minZ), // C
                new Vector3(minX, minY, minZ), // D
                new Vector3(minX, maxY, maxZ), // E
                new Vector3(maxX, maxY, maxZ), // F
                new Vector3(minX, minY, maxZ), // G
                new Vector3(maxX, minY, maxZ)  // H
            };
        }

        public override Vector3 GetCenterOfAABB(Vector3[] aabb)
        {
            float[] x = new float[] { aabb[1].X, aabb[0].X };
            float[] y = new float[] { aabb[0].Y, aabb[2].Y };
            float[] z = new float[] { aabb[4].Z, aabb[0].Z };
            return new Vector3(x.Average(), y.Average(), z.Average());
        }

        //

        public static OBJModel LoadFromFile(string filename)
        {
            OBJModel mdl = new OBJModel();
            try
            {
                using (StreamReader reader = new StreamReader(new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read)))
                {
                    mdl = LoadFromString(reader.ReadToEnd());
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

            return mdl;
        }

        public static OBJModel LoadFromString(string obj)
        {
            List<string> lines = new List<string>(obj.Split('\n'));

            // Lists to hold model data
            List<Vector3> verts = new List<Vector3>();
            List<Vector3> normals = new List<Vector3>();
            List<Vector2> texs = new List<Vector2>();
            List<Tuple<TempVertex, TempVertex, TempVertex>> faces = new List<Tuple<TempVertex, TempVertex, TempVertex>>();

            // Base values
            verts.Add(new Vector3());
            texs.Add(new Vector2());
            normals.Add(new Vector3());

            //int currentindice = 0;

            // Read file line by line
            foreach (string line in lines)
            {
                if (line.StartsWith("v ")) // Vertex definition
                {
                    // Cut off beginning of line
                    string temp = line.Substring(2);

                    Vector3 vec = new Vector3();

                    if (temp.Trim().Count((char c) => c == ' ') == 2) // Check if there's enough elements for a vertex
                    {
                        string[] vertparts = temp.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                        // Attempt to parse each part of the vertice
                        bool success = float.TryParse(vertparts[0], out vec.X);
                        success |= float.TryParse(vertparts[1], out vec.Y);
                        success |= float.TryParse(vertparts[2], out vec.Z);

                        // If any of the parses failed, report the error
                        if (!success)
                        {
                            Console.WriteLine("Error parsing vertex: {0}", line);
                        }
                    }
                    else
                    {
                        Console.WriteLine("Error parsing vertex: {0}", line);
                    }

                    verts.Add(vec);
                }
                else if (line.StartsWith("vt ")) // Texture coordinate
                {
                    // Cut off beginning of line
                    string temp = line.Substring(2);

                    Vector2 vec = new Vector2();

                    if (temp.Trim().Count((char c) => c == ' ') > 0) // Check if there's enough elements for a vertex
                    {
                        string[] texcoordparts = temp.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                        // Attempt to parse each part of the vertice
                        bool success = float.TryParse(texcoordparts[0], out vec.X);
                        success |= float.TryParse(texcoordparts[1], out vec.Y);

                        // If any of the parses failed, report the error
                        if (!success)
                        {
                            Console.WriteLine("Error parsing texture coordinate: {0}", line);
                        }
                    }
                    else
                    {
                        Console.WriteLine("Error parsing texture coordinate: {0}", line);
                    }

                    texs.Add(vec);
                }
                else if (line.StartsWith("vn ")) // Normal vector
                {
                    // Cut off beginning of line
                    string temp = line.Substring(2);

                    Vector3 vec = new Vector3();

                    if (temp.Trim().Count((char c) => c == ' ') == 2) // Check if there's enough elements for a normal
                    {
                        string[] vertparts = temp.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                        // Attempt to parse each part of the vertice
                        bool success = float.TryParse(vertparts[0], out vec.X);
                        success |= float.TryParse(vertparts[1], out vec.Y);
                        success |= float.TryParse(vertparts[2], out vec.Z);

                        // If any of the parses failed, report the error
                        if (!success)
                        {
                            Console.WriteLine("Error parsing normal: {0}", line);
                        }
                    }
                    else
                    {
                        Console.WriteLine("Error parsing normal: {0}", line);
                    }

                    normals.Add(vec);
                }
                else if (line.StartsWith("f ")) // Face definition
                {
                    // Cut off beginning of line
                    string temp = line.Substring(2);

                    Tuple<TempVertex, TempVertex, TempVertex> face = new Tuple<TempVertex, TempVertex, TempVertex>(new TempVertex(), new TempVertex(), new TempVertex());

                    if (temp.Trim().Count((char c) => c == ' ') == 2) // Check if there's enough elements for a face
                    {
                        string[] faceparts = temp.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                        int v1, v2, v3;
                        int t1, t2, t3;
                        int n1, n2, n3;

                        // Attempt to parse each part of the face
                        bool success = int.TryParse(faceparts[0].Split('/')[0], out v1);
                        success |= int.TryParse(faceparts[1].Split('/')[0], out v2);
                        success |= int.TryParse(faceparts[2].Split('/')[0], out v3);

                        if (faceparts[0].Count((char c) => c == '/') >= 2)
                        {
                            success |= int.TryParse(faceparts[0].Split('/')[1], out t1);
                            success |= int.TryParse(faceparts[1].Split('/')[1], out t2);
                            success |= int.TryParse(faceparts[2].Split('/')[1], out t3);
                            success |= int.TryParse(faceparts[0].Split('/')[2], out n1);
                            success |= int.TryParse(faceparts[1].Split('/')[2], out n2);
                            success |= int.TryParse(faceparts[2].Split('/')[2], out n3);
                        }
                        else
                        {
                            if (texs.Count > v1 && texs.Count > v2 && texs.Count > v3)
                            {
                                t1 = v1;
                                t2 = v2;
                                t3 = v3;
                            }
                            else
                            {
                                t1 = 0;
                                t2 = 0;
                                t3 = 0;
                            }

                            if (normals.Count > v1 && normals.Count > v2 && normals.Count > v3)
                            {
                                n1 = v1;
                                n2 = v2;
                                n3 = v3;
                            }
                            else
                            {
                                n1 = 0;
                                n2 = 0;
                                n3 = 0;
                            }
                        }

                        // If any of the parses failed, report the error
                        if (!success)
                        {
                            Console.WriteLine("Error parsing face: {0}", line);
                        }
                        else
                        {
                            TempVertex tv1 = new TempVertex(v1, n1, t1);
                            TempVertex tv2 = new TempVertex(v2, n2, t2);
                            TempVertex tv3 = new TempVertex(v3, n3, t3);
                            face = new Tuple<TempVertex, TempVertex, TempVertex>(tv1, tv2, tv3);
                            faces.Add(face);
                        }
                    }
                    else
                    {
                        Console.WriteLine("Error parsing face: {0}", line);
                    }
                }
            }

            OBJModel mdl = new OBJModel();
            foreach (var face in faces)
            {
                if (face.Item1.Vertex <= verts.Count && face.Item2.Vertex <= verts.Count && face.Item3.Vertex <= verts.Count)
                {
                    FaceVertex v1 = new FaceVertex(verts[face.Item1.Vertex], normals[face.Item1.Normal], texs[face.Item1.Texcoord]);
                    FaceVertex v2 = new FaceVertex(verts[face.Item2.Vertex], normals[face.Item2.Normal], texs[face.Item2.Texcoord]);
                    FaceVertex v3 = new FaceVertex(verts[face.Item3.Vertex], normals[face.Item3.Normal], texs[face.Item3.Texcoord]);
                    mdl.faces.Add(new Tuple<FaceVertex, FaceVertex, FaceVertex>(v1, v2, v3));
                }
            }
            return mdl;
        }
    }
}