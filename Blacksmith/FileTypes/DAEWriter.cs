using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

// adapted from: https://github.com/Ploaj/SSBHLib/blob/fe395033f4/CrossMod/IO/DAEWriter.cs

namespace Blacksmith.Three
{
    public class DAEWriter : IDisposable
    {
        public enum VERTEX_SEMANTIC
        {
            POSITION,
            NORMAL,
            TEXCOORD,
            COLOR
        }

        public enum SKIN_SEMANTIC
        {
            JOINT,
            INV_BIND_MATRIX,
            WEIGHT
        }

        private class JOINT
        {
            public string Name;
            public int Parent = -1;
            public float[] Transform;
            public float[] BindPose;
        }

        private XmlTextWriter writer;

        private bool Optimize = false;

        // for keeping track of used is so there are no repeats
        private Dictionary<string, int> UsedIDs = new Dictionary<string, int>();

        // for writing geometry
        public string CurrentGeometryID;
        public string CurrentMaterial;
        private List<Tuple<string, VERTEX_SEMANTIC, uint[], int>> GeometrySources = new List<Tuple<string, VERTEX_SEMANTIC, uint[], int>>();
        private Dictionary<string, Tuple<List<int[]>, List<float[]>>> GeometryControllers = new Dictionary<string, Tuple<List<int[]>, List<float[]>>>();
        private Dictionary<string, string> MeshToSkinLink = new Dictionary<string, string>();
        private Dictionary<string, string> MeshIdToMeshName = new Dictionary<string, string>();
        private Dictionary<string, string> MeshIdToMaterial = new Dictionary<string, string>();

        // for writing bones
        private List<JOINT> Joints = new List<JOINT>();

        /// <summary>
        /// Creates a new DAEWriter
        /// </summary>
        /// <param name="FileName"></param>
        public DAEWriter(string FileName, bool Optimize = false)
        {
            writer = new XmlTextWriter(FileName, Encoding.UTF8);
            writer.Formatting = Formatting.Indented;
            writer.Indentation = 2;
            this.Optimize = Optimize;
            WriteHeader();
        }

        /// <summary>
        /// Gets a unique identifier for given string by appending a number onto the based on 
        /// how many times that given identifier has been used
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public string GetUniqueID(string id)
        {
            if (UsedIDs.ContainsKey(id))
            {
                UsedIDs[id]++;
                return $"{id}_{UsedIDs[id]}";//
            }
            else
            {
                UsedIDs.Add(id, 0);
                return id;
            }
        }

        /// <summary>
        /// Writes the header data of the DAE file
        /// This needs to be written first
        /// </summary>
        public void WriteHeader()
        {
            writer.WriteStartDocument();
            writer.WriteStartElement("COLLADA");
            writer.WriteAttributeString("xmlns", "http://www.collada.org/2005/11/COLLADASchema");
            writer.WriteAttributeString("version", "1.4.1");
            //writer.WriteAttributeString("xsi", "http://www.w3.org/2001/XMLSchema-instance");
        }

        public void WriteAsset()
        {
            writer.WriteStartElement("asset");
            writer.WriteEndElement();
        }


        public void WriteLibraryImages(string[] TextureNames = null)
        {
            writer.WriteStartElement("library_images");
            if (TextureNames != null)
            {
                foreach (var tn in TextureNames)
                {
                    writer.WriteStartElement("image");
                    writer.WriteAttributeString("id", tn);
                    writer.WriteStartElement("init_from");
                    writer.WriteString(tn + ".png");
                    writer.WriteEndElement();
                    writer.WriteEndElement();
                }
            }
            writer.WriteEndElement();
        }


        /// <summary>
        /// Begins writing the material section
        /// </summary>
        public void StartMaterialSection()
        {
            writer.WriteStartElement("library_materials");
        }

        /// <summary>
        /// Writes a material information with an effect reference
        /// </summary>
        /// <param name="Name"></param>
        public void WriteMaterial(string Name)
        {
            writer.WriteStartElement("material");
            writer.WriteAttributeString("id", Name);
            writer.WriteStartElement("instance_effect");
            writer.WriteAttributeString("url", "#Effect_" + Name);
            writer.WriteEndElement();
            writer.WriteEndElement();
        }

        /// <summary>
        /// Ends writing the material section
        /// </summary>
        public void EndMaterialSection()
        {
            writer.WriteEndElement();
        }

        /// <summary>
        /// Begins writing the effect section
        /// </summary>
        public void StartEffectSection()
        {
            writer.WriteStartElement("library_effects");
        }

        /// <summary>
        /// Writes a effect
        /// </summary>
        public void WriteEffect(string Name, string DiffuseTextureName)
        {
            writer.WriteStartElement("effect");
            writer.WriteAttributeString("id", "Effect_" + Name);
            writer.WriteStartElement("profile_COMMON");

            {
                writer.WriteStartElement("newparam");
                writer.WriteAttributeString("sid", "surface_" + Name);
                writer.WriteStartElement("surface");
                writer.WriteAttributeString("type", "2D");
                {
                    writer.WriteStartElement("init_from");
                    writer.WriteString(DiffuseTextureName);
                    writer.WriteEndElement();

                    writer.WriteStartElement("format");
                    writer.WriteString("A8R8G8B8");
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
                writer.WriteEndElement();
            }


            {
                writer.WriteStartElement("newparam");
                writer.WriteAttributeString("sid", "sampler_" + Name);
                writer.WriteStartElement("sampler2D");
                {
                    writer.WriteStartElement("source");
                    writer.WriteString("surface_" + Name);
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
                writer.WriteEndElement();
            }

            {
                writer.WriteStartElement("technique");
                writer.WriteAttributeString("sid", "common");
                writer.WriteStartElement("phong");
                {
                    writer.WriteStartElement("diffuse");
                    writer.WriteStartElement("texture");
                    writer.WriteAttributeString("texture", "sampler_" + Name);
                    writer.WriteAttributeString("texcoord", "");
                    writer.WriteEndElement();
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
                writer.WriteEndElement();
            }

            writer.WriteEndElement();
            writer.WriteEndElement();
        }

        /// <summary>
        /// Ends writing the effect section
        /// </summary>
        public void EndEffectSection()
        {
            writer.WriteEndElement();
        }

        /// <summary>
        /// Begins writing the Geometry Section
        /// </summary>
        public void StartGeometrySection()
        {
            writer.WriteStartElement("library_geometries");
        }

        /// <summary>
        /// Starts writing a geometry mesh
        /// </summary>
        public void StartGeometryMesh(string Name)
        {
            CurrentGeometryID = GetUniqueID(Name + "-mesh");
            MeshToSkinLink.Add(CurrentGeometryID, "");
            MeshIdToMeshName.Add(CurrentGeometryID, Name);
            writer.WriteStartElement("geometry");
            writer.WriteAttributeString("id", CurrentGeometryID);
            writer.WriteAttributeString("name", Name);
            writer.WriteStartElement("mesh");
        }

        /// <summary>
        /// Provides a container for using arrays of values as keys in <see cref="Dictionary{TKey, TValue}"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        private class ValueContainer<T> : IEquatable<ValueContainer<T>> where T : struct
        {
            public T[] Values { get; }
            private readonly int hashCode;

            public ValueContainer(T[] values)
            {
                Values = values;
                hashCode = ((System.Collections.IStructuralEquatable)Values).GetHashCode(EqualityComparer<T>.Default);
            }

            public override bool Equals(object obj)
            {
                return Equals(obj as ValueContainer<T>);
            }

            public bool Equals(ValueContainer<T> other)
            {
                // Compare precalculated hash first for performance reasons.
                // The entire sequence needs to be compared to resolve collisions.
                return other != null && hashCode == other.hashCode && Enumerable.SequenceEqual(Values, other.Values);
            }

            public override int GetHashCode()
            {
                return hashCode;
            }
        }

        /// <summary>
        /// Super slow
        /// </summary>
        /// <param name="values"></param>
        /// <param name="indices"></param>
        /// <param name="stride"></param>
        /// <param name="newValues"></param>
        /// <param name="newIndices"></param>
        private void OptimizeSource<T>(T[] values, uint[] indices, int stride, out T[] newValues, out uint[] newIndices) where T : struct
        {
            var optimizedIndices = new List<uint>();
            var optimizedValues = new List<T>();

            // Use a special class to store values, so we can have the performance benefits of a dictionary.
            var vertexBank = new Dictionary<ValueContainer<T>, uint>();

            for (int i = 0; i < indices.Length; i++)
            {
                var vertexValues = new ValueContainer<T>(GetVertexValues(values, indices, stride, i));

                if (!vertexBank.ContainsKey(vertexValues))
                {
                    uint index = (uint)vertexBank.Count;
                    optimizedIndices.Add(index);
                    vertexBank.Add(vertexValues, index);
                    optimizedValues.AddRange(vertexValues.Values);
                }
                else
                {
                    optimizedIndices.Add(vertexBank[vertexValues]);

                }
            }

            newIndices = optimizedIndices.ToArray();
            newValues = optimizedValues.ToArray();
        }

        private static T[] GetVertexValues<T>(T[] values, uint[] indices, int stride, int i)
        {
            var vertexValues = new T[stride];
            for (int j = 0; j < stride; j++)
                vertexValues[j] = values[indices[i] * stride + j];

            return vertexValues;
        }

        /// <summary>
        /// Writes a geometry source to file
        /// </summary>
        /// <param name="Name"></param>
        /// <param name="Semantic"></param>
        /// <param name="Values"></param>
        /// <param name="set"></param>
        public void WriteGeometrySource(string Name, VERTEX_SEMANTIC Semantic, float[] Values, uint[] Indices, int set = -1)
        {
            int Stride = 3;
            if (Semantic == VERTEX_SEMANTIC.TEXCOORD)
                Stride = 2;
            if (Semantic == VERTEX_SEMANTIC.COLOR)
                Stride = 4;

            if (Optimize)
                OptimizeSource(Values, Indices, Stride, out Values, out Indices);

            string sourceid = GetUniqueID(Name + "-" + Semantic.ToString().ToLower());
            writer.WriteStartElement("source");
            writer.WriteAttributeString("id", sourceid);

            writer.WriteStartElement("float_array");
            string FloatArrayID = GetUniqueID(Name + "-" + Semantic.ToString().ToLower() + "-array");
            writer.WriteAttributeString("id", FloatArrayID);
            writer.WriteAttributeString("count", Values.Length.ToString());
            writer.WriteString(string.Join(" ", Values));
            writer.WriteEndElement();

            writer.WriteStartElement("technique_common");
            {
                writer.WriteStartElement("accessor");
                writer.WriteAttributeString("source", $"#{FloatArrayID}");
                writer.WriteAttributeString("count", (Values.Length / Stride).ToString());
                writer.WriteAttributeString("stride", Stride.ToString());
                if (Semantic == VERTEX_SEMANTIC.NORMAL || Semantic == VERTEX_SEMANTIC.POSITION)
                {
                    WriteParam("X", "float");
                    WriteParam("Y", "float");
                    WriteParam("Z", "float");
                }
                if (Semantic == VERTEX_SEMANTIC.TEXCOORD)
                {
                    WriteParam("S", "float");
                    WriteParam("T", "float");
                }
                if (Semantic == VERTEX_SEMANTIC.COLOR)
                {
                    WriteParam("R", "float");
                    WriteParam("G", "float");
                    WriteParam("B", "float");
                    WriteParam("A", "float");
                }
                writer.WriteEndElement();
            }
            writer.WriteEndElement();

            writer.WriteEndElement();

            GeometrySources.Add(new Tuple<string, VERTEX_SEMANTIC, uint[], int>(sourceid, Semantic, Indices, set));
        }

        /// <summary>
        /// Writes param info
        /// used for writing source accessors
        /// </summary>
        /// <param name="Name"></param>
        /// <param name="Type"></param>
        private void WriteParam(string Name, string Type)
        {
            writer.WriteStartElement("param");
            writer.WriteAttributeString("name", Name);
            writer.WriteAttributeString("type", Type);
            writer.WriteEndElement();
        }


        /// <summary>
        /// Ends writing a geometry mesh
        /// </summary>
        public void EndGeometryMesh()
        {
            Tuple<string, VERTEX_SEMANTIC, uint[], int> Position = null;
            foreach (var v in GeometrySources)
            {
                if (v.Item2 == VERTEX_SEMANTIC.POSITION)
                {
                    Position = v;
                    break;
                }
            }

            // can only write vertex information if there is at least position data
            if (Position != null)
            {
                // vertices
                string verticesid = GetUniqueID(Position.Item1.Replace("position", "vertex"));
                writer.WriteStartElement("vertices");
                writer.WriteAttributeString("id", verticesid);
                WriteInput(Position.Item2.ToString(), Position.Item1);
                writer.WriteEndElement();

                // triangles
                writer.WriteStartElement("triangles");
                if (CurrentMaterial != "")
                {
                    writer.WriteAttributeString("material", $"{CurrentMaterial}");
                    MeshIdToMaterial.Add(CurrentGeometryID, CurrentMaterial);
                    CurrentMaterial = "";
                }
                writer.WriteAttributeString("count", Position.Item3.Length.ToString());
                WriteInput("VERTEX", $"{verticesid}", 0);
                int offset = 1;
                foreach (var v in GeometrySources)
                    if (v.Item2 != VERTEX_SEMANTIC.POSITION)
                    {
                        WriteInput(v.Item2.ToString(), $"{v.Item1}", offset++, v.Item4);
                    }
                // write p
                StringBuilder p = new StringBuilder();
                for (int i = 0; i < Position.Item3.Length; i++)
                {
                    p.Append(Position.Item3[i] + " ");
                    foreach (var v in GeometrySources)
                        if (v.Item2 != VERTEX_SEMANTIC.POSITION)
                            p.Append(v.Item3[i] + " ");
                }
                writer.WriteStartElement("p");
                writer.WriteString(p.ToString());
                writer.WriteEndElement();

                writer.WriteEndElement();
            }

            GeometrySources.Clear();
            writer.WriteEndElement();
            writer.WriteEndElement();
        }

        /// <summary>
        /// Write an input node onto the current element
        /// </summary>
        /// <param name="semantic"></param>
        /// <param name="sourceid"></param>
        /// <param name="offset"></param>
        /// <param name="set"></param>
        public void WriteInput(string semantic, string sourceid, int offset = -1, int set = -1)
        {
            writer.WriteStartElement("input");
            writer.WriteAttributeString("semantic", semantic);
            writer.WriteAttributeString("source", $"#{sourceid}");
            if (offset != -1)
                writer.WriteAttributeString("offset", offset.ToString());
            if (set != -1)
                writer.WriteAttributeString("set", set.ToString());
            writer.WriteEndElement();
        }

        /// <summary>
        /// A function for automatically creating and handling the controllers
        /// The Joints must be added before this function is used
        /// </summary>
        /// <param name="BoneIndices">A list of arrays of bone indices for each vertex</param>
        /// <param name="Weights">A list that contains an array of weights per vertex</param>
        public void AttachGeometryController(List<int[]> BoneIndices, List<float[]> Weights)
        {
            GeometryControllers.Add(CurrentGeometryID, new Tuple<List<int[]>, List<float[]>>(BoneIndices, Weights));
        }

        /// <summary>
        /// Ends Writing the Geometry Section
        /// </summary>
        public void EndGeometrySection()
        {
            writer.WriteEndElement();

            CreateControllerSection();

            CreateVisualNodeSection();
        }

        private void RecursivlyWriteJoints(JOINT joint)
        {
            writer.WriteStartElement("node");
            writer.WriteAttributeString("id", $"Armature_{joint.Name}");
            writer.WriteAttributeString("name", joint.Name);
            writer.WriteAttributeString("sid", joint.Name);
            writer.WriteAttributeString("type", "JOINT");

            writer.WriteStartElement("matrix");
            writer.WriteAttributeString("sid", "transform");
            writer.WriteString(string.Join(" ", joint.Transform));
            writer.WriteEndElement();

            foreach (var child in GetChildren(joint))
            {
                RecursivlyWriteJoints(child);
            }

            writer.WriteEndElement();
        }

        private JOINT[] GetChildren(JOINT j)
        {
            int parentindex = Joints.IndexOf(j);
            List<JOINT> Children = new List<JOINT>();
            foreach (var child in Joints)
            {
                if (child.Parent == parentindex)
                    Children.Add(child);
            }
            return Children.ToArray();
        }

        /// <summary>
        /// Starts the library controller section
        /// </summary>
        public void BeginLibraryControllers()
        {
            writer.WriteStartElement("library_controllers");
        }

        /// <summary>
        /// automatically generates the controller nodes
        /// </summary>
        private void CreateControllerSection()
        {
            BeginLibraryControllers();

            foreach (var v in GeometryControllers.Keys)
            {
                WriteLibraryController(v);
            }

            EndLibraryControllers();
        }


        /// <summary>
        /// Adds a new joint to the default skeletal tree
        /// </summary>
        public void AddJoint(string name, string parentName, float[] Transform, float[] InvWorldTransform)
        {
            JOINT j = new JOINT();
            j.Name = name;
            j.Transform = Transform;
            j.BindPose = InvWorldTransform;
            foreach (var joint in Joints)
                if (joint.Name.Equals(parentName))
                    j.Parent = Joints.IndexOf(joint);
            Joints.Add(j);
        }

        public void WriteLibraryController(string Name)
        {
            if (!GeometryControllers.ContainsKey(Name)) return;

            var BoneWeight = GeometryControllers[Name];
            string SkinID = $"Armature_{Name}";
            MeshToSkinLink[Name] = SkinID;

            writer.WriteStartElement("controller");
            writer.WriteAttributeString("id", SkinID);

            writer.WriteStartElement("skin");
            writer.WriteAttributeString("source", $"#{Name}");
            {
                writer.WriteStartElement("bind_shape_matrix");
                writer.WriteString("1 0 0 0 0 1 0 0 0 0 1 0 0 0 0 1");
                writer.WriteEndElement();

                object[] BoneNames = new string[Joints.Count];
                object[] InvBinds = new object[Joints.Count * 16];
                for (int i = 0; i < BoneNames.Length; i++)
                {
                    BoneNames[i] = Joints[i].Name;
                    for (int j = 0; j < 16; j++)
                    {
                        InvBinds[i * 16 + j] = Joints[i].BindPose[j];
                    }
                }
                var Weights = new List<object>();
                var WeightIndices = new List<int>();
                foreach (var v in BoneWeight.Item2)
                {
                    foreach (var w in v)
                    {
                        int index = Weights.IndexOf(w);
                        if (index == -1)
                        {
                            WeightIndices.Add(Weights.Count);
                            Weights.Add(w);
                        }
                        else
                        {
                            WeightIndices.Add(index);
                        }
                    }
                }
                string Jointid = WriteSkinSource(Name, SKIN_SEMANTIC.JOINT, BoneNames);
                string Bindid = WriteSkinSource(Name, SKIN_SEMANTIC.INV_BIND_MATRIX, InvBinds);
                string Weightid = WriteSkinSource(Name, SKIN_SEMANTIC.WEIGHT, Weights.ToArray());


                writer.WriteStartElement("joints");
                WriteInput(SKIN_SEMANTIC.JOINT.ToString(), Jointid);
                WriteInput(SKIN_SEMANTIC.INV_BIND_MATRIX.ToString(), Bindid);
                writer.WriteEndElement();

                writer.WriteStartElement("vertex_weights");
                writer.WriteAttributeString("count", BoneWeight.Item1.Count.ToString());
                WriteInput(SKIN_SEMANTIC.JOINT.ToString(), Jointid, 0);
                WriteInput(SKIN_SEMANTIC.WEIGHT.ToString(), Weightid, 1);

                // now writing out the counts and such...
                //vcount
                {
                    StringBuilder values = new StringBuilder();
                    foreach (var v in BoneWeight.Item1)
                    {
                        values.Append($"{v.Length} ");
                    }
                    writer.WriteStartElement("vcount");
                    writer.WriteString(values.ToString());
                    writer.WriteEndElement();
                }

                //v
                {
                    StringBuilder values = new StringBuilder();
                    int weightindexcount = 0;
                    for (int i = 0; i < BoneWeight.Item1.Count; i++)
                    {
                        for (int j = 0; j < BoneWeight.Item1[i].Length; j++)
                        {
                            values.Append($"{BoneWeight.Item1[i][j]} {WeightIndices[weightindexcount++]} ");
                        }
                    }
                    writer.WriteStartElement("v");
                    writer.WriteString(values.ToString());
                    writer.WriteEndElement();
                }

                writer.WriteEndElement();
            }

            writer.WriteEndElement();
            writer.WriteEndElement();
        }

        /// <summary>
        /// Writes a skin source accessor for the current skin controller
        /// </summary>
        /// <param name="Name"></param>
        /// <param name="Semantic"></param>
        /// <param name="Values"></param>
        /// <returns>the id of the newly created skin source</returns>
        public string WriteSkinSource(string Name, SKIN_SEMANTIC Semantic, object[] Values)
        {
            int Stride = 0;
            switch (Semantic)
            {
                case SKIN_SEMANTIC.JOINT:
                    Stride = 1;
                    break;
                case SKIN_SEMANTIC.INV_BIND_MATRIX:
                    Stride = 16;
                    break;
                case SKIN_SEMANTIC.WEIGHT:
                    Stride = 1;
                    break;
            }

            string sourceid = GetUniqueID(Name + "-" + Semantic.ToString().ToLower());
            writer.WriteStartElement("source");
            writer.WriteAttributeString("id", sourceid);

            writer.WriteStartElement(Semantic == SKIN_SEMANTIC.JOINT ? "Name_array" : "float_array");
            string FloatArrayID = GetUniqueID(Name + "-" + Semantic.ToString().ToLower() + "-array");
            writer.WriteAttributeString("id", FloatArrayID);
            writer.WriteAttributeString("count", Values.Length.ToString());
            writer.WriteString(string.Join(" ", Values));
            writer.WriteEndElement();

            writer.WriteStartElement("technique_common");
            {
                writer.WriteStartElement("accessor");
                writer.WriteAttributeString("source", $"#{FloatArrayID}");
                writer.WriteAttributeString("count", (Values.Length / Stride).ToString());
                writer.WriteAttributeString("stride", Stride.ToString());
                if (Semantic == SKIN_SEMANTIC.JOINT)
                {
                    WriteParam("JOINT", "name");
                }
                if (Semantic == SKIN_SEMANTIC.INV_BIND_MATRIX)
                {
                    WriteParam("TRANSFORM", "float4x4");
                }
                if (Semantic == SKIN_SEMANTIC.WEIGHT)
                {
                    WriteParam("WEIGHT", "float");
                }
                writer.WriteEndElement();
            }
            writer.WriteEndElement();

            writer.WriteEndElement();

            return sourceid;
        }

        /// <summary>
        /// Ends the Library Controller Section
        /// </summary>
        public void EndLibraryControllers()
        {
            writer.WriteEndElement();
        }

        /// <summary>
        /// Begins the visual scene section
        /// </summary>
        public void BeginVisualNodeSection()
        {
            writer.WriteStartElement("library_visual_scenes");
            writer.WriteStartElement("visual_scene");
            writer.WriteAttributeString("id", "Scene");
            writer.WriteAttributeString("name", "Scene");
        }

        /// <summary>
        /// Automatically creates and appends the visual node section
        /// </summary>
        public void CreateVisualNodeSection()
        {
            BeginVisualNodeSection();
            writer.WriteStartElement("node");
            writer.WriteAttributeString("id", "Armature");
            writer.WriteAttributeString("name", "Armature");
            writer.WriteAttributeString("type", "NODE");

            writer.WriteStartElement("matrix");
            writer.WriteAttributeString("sid", "transform");
            writer.WriteString("1 0 0 0 0 1 0 0 0 0 1 0 0 0 0 1");
            writer.WriteEndElement();

            foreach (var joint in Joints)
                if (joint.Parent == -1)
                    RecursivlyWriteJoints(joint);

            writer.WriteEndElement();

            // write geometry nodes

            foreach (var m in MeshToSkinLink)
            {
                writer.WriteStartElement("node");
                writer.WriteAttributeString("id", MeshIdToMeshName[m.Key]);
                writer.WriteAttributeString("name", MeshIdToMeshName[m.Key]);
                writer.WriteAttributeString("type", "NODE");

                if (m.Value.Equals(""))
                {
                    writer.WriteStartElement("instance_geometry");
                    writer.WriteAttributeString("url", $"#{m.Key}");
                    writer.WriteAttributeString("name", MeshIdToMeshName[m.Key]);
                    writer.WriteEndElement();
                }
                else
                {
                    writer.WriteStartElement("instance_controller");
                    writer.WriteAttributeString("url", $"#{m.Value}");
                    writer.WriteStartElement("skeleton");
                    writer.WriteString("#Armature_" + Joints[0].Name);
                    writer.WriteEndElement();
                    if (MeshIdToMaterial.ContainsKey(m.Key))
                    {
                        writer.WriteStartElement("bind_material");
                        writer.WriteStartElement("technique_common");
                        writer.WriteStartElement("instance_material");
                        writer.WriteAttributeString("symbol", MeshIdToMaterial[m.Key]);
                        writer.WriteAttributeString("target", "#" + MeshIdToMaterial[m.Key]);
                        writer.WriteEndElement();
                        writer.WriteEndElement();
                        writer.WriteEndElement();
                    }
                    writer.WriteEndElement();
                }


                writer.WriteEndElement();
            }

            EndVisualNodeSection();
        }

        public void WriteJoint()
        {

        }

        public void WriteNode(string Name)
        {
            writer.WriteStartElement("node");
            writer.WriteAttributeString("id", Name);
            writer.WriteAttributeString("name", Name);
            writer.WriteAttributeString("type", "NODE");
            writer.WriteEndElement();
        }

        /// <summary>
        /// ends the visual scene section
        /// </summary>
        public void EndVisualNodeSection()
        {
            writer.WriteEndElement();
            writer.WriteEndElement();

            writer.WriteStartElement("scene");
            writer.WriteStartElement("instance_visual_scene");
            writer.WriteAttributeString("url", "#Scene");
            writer.WriteEndElement();
            writer.WriteEndElement();
        }

        /// <summary>
        /// Close the stream when done
        /// </summary>
        public void Dispose()
        {
            if (writer != null)
            {
                writer.WriteEndElement();
                writer.WriteEndDocument();
                writer.Close();
            }
        }

    }
}