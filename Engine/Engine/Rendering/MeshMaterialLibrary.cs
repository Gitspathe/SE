using System;
using System.Collections.Generic;
using DeferredEngine.Renderer.RenderModules.Default;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SE.Common;
using SE.Components;
using SE.Core;

namespace SE.Rendering
{
    public class MeshMaterialLibrary
    {
        const int InitialLibrarySize = 10;
        public MaterialLibrary[] MaterialLib = new MaterialLibrary[InitialLibrarySize];

        public int[] MaterialLibPointer = new int[InitialLibrarySize];

        public int Index;

        private bool _previousMode = GameSettings.g_cpuculling;
        private bool _previousEditorMode = GameSettings.e_enableeditor;
        private readonly BoundingSphere _defaultBoundingSphere;
        private RasterizerState _shadowGenerationRasterizerState;
        private FullScreenTriangle _fullScreenTriangle;
        private DepthStencilState _depthWrite;

        private GraphicsDevice graphicsDevice;

        public MeshMaterialLibrary(GraphicsDevice graphics)
        {
            graphicsDevice = graphics;

            _defaultBoundingSphere = new BoundingSphere(Vector3.Zero, 0);

            _shadowGenerationRasterizerState = new RasterizerState()
            {
                CullMode = CullMode.CullCounterClockwiseFace,
                ScissorTestEnable = true
            };

            _depthWrite = new DepthStencilState()
            {
                DepthBufferEnable = true,
                DepthBufferWriteEnable = true,
                DepthBufferFunction = CompareFunction.Always
            };

            _fullScreenTriangle = new FullScreenTriangle(graphics);
        }

        public void Register(MaterialEffect mat, Model model, Transform worldMatrix)
        {
            if (model == null) return;

            //if (mat == null)
            //{
            //    throw new NotImplementedException();
            //}

            for (int index = 0; index < model.Meshes.Count; index++)
            {
                var mesh = model.Meshes[index];
                for (int i = 0; i < mesh.MeshParts.Count; i++)
                {
                    ModelMeshPart meshPart = mesh.MeshParts[i];
                    Register(mat, meshPart, worldMatrix, mesh.BoundingSphere);
                }
            }
        }

        public void Register(MaterialEffect mat, ModelMeshPart mesh, Transform worldMatrix, BoundingSphere boundingSphere) //These should be ordered by likeness, so I don't get opaque -> transparent -> opaque
        {
            bool found = false;

            if (mat == null)
            {
                var effect = mesh.Effect as MaterialEffect;
                if (effect != null)
                {
                    mat = effect;
                }
                else
                {
                    mat = new MaterialEffect(mesh.Effect);
                }
            }

            //Check if we already have a material like that, if yes put it in there!
            for (var i = 0; i < Index; i++)
            {
                MaterialLibrary matLib = MaterialLib[i];
                if (matLib.HasMaterial(mat))
                {
                    matLib.Register(mesh, worldMatrix, boundingSphere);
                    found = true;
                    break;
                }
            }

            //We have no MatLib for that specific Material yet. Make a new one.
            if (!found)
            {
                MaterialLib[Index] = new MaterialLibrary();
                MaterialLib[Index].SetMaterial(ref mat);
                MaterialLib[Index].Register(mesh, worldMatrix, boundingSphere);
                Index++;
            }

            //If we exceeded our array length, make the array bigger.
            if (Index >= MaterialLib.Length)
            {
                MaterialLibrary[] tempLib = new MaterialLibrary[Index + 1];
                MaterialLib.CopyTo(tempLib, 0);
                MaterialLib = tempLib;

                MaterialLibPointer = new int[Index + 1];
                //sort from 0 to Index
                for (int j = 0; j < MaterialLibPointer.Length; j++)
                {
                    MaterialLibPointer[j] = j;
                }
                SortByDistance();
            }
        }

        //Not a real sort, but it does it's job over time
        private void SortByDistance()
        {
            if (!GameSettings.g_cpusort) return;

            for (int i = 1; i < Index; i++)
            {
                float distanceI = MaterialLib[MaterialLibPointer[i]].DistanceSquared;
                float distanceJ = MaterialLib[MaterialLibPointer[i - 1]].DistanceSquared;

                if (distanceJ < distanceI)
                {
                    //swap
                    int temp = MaterialLibPointer[i];
                    MaterialLibPointer[i] = MaterialLibPointer[i - 1];
                    MaterialLibPointer[i - 1] = temp;
                }
            }
        }

        public void DeleteFromRegistry(ModelRenderer basicEntity)
        {
            if (basicEntity.ModelDefinition.Model == null) return; //nothing to delete

            //delete the individual meshes!
            for (int index = 0; index < basicEntity.ModelDefinition.Model.Meshes.Count; index++)
            {
                var mesh = basicEntity.ModelDefinition.Model.Meshes[index];
                for (int i = 0; i < mesh.MeshParts.Count; i++)
                {
                    ModelMeshPart meshPart = mesh.MeshParts[i];
                    DeleteFromRegistry(basicEntity.Material, meshPart, basicEntity.ModelDefinition.Transform);
                }
            }
        }

        private void DeleteFromRegistry(MaterialEffect mat, ModelMeshPart mesh, Transform worldMatrix)
        {
            for (var i = 0; i < Index; i++)
            {
                MaterialLibrary matLib = MaterialLib[i];
                //if (matLib.HasMaterial(mat))
                //{
                if (matLib.DeleteFromRegistry(mesh, worldMatrix))
                {
                    for (var j = i; j < Index - 1; j++)
                    {
                        //slide down one
                        MaterialLib[j] = MaterialLib[j + 1];

                    }
                    Index--;

                    break;
                }
                //}
            }
        }

        /// <summary>
        /// Update whether or not Objects are in the viewFrustumEx and need to be rendered or not.
        /// </summary>
        /// <param name="entities"></param>
        /// <param name="boundingFrustrum"></param>
        /// <param name="hasCameraChanged"></param>
        public bool FrustumCulling(List<ModelRenderer> entities, BoundingFrustum boundingFrustrum, bool hasCameraChanged, Vector3 cameraPosition)
        {
            //Check if the culling mode has changed
            if (_previousMode != GameSettings.g_cpuculling)
            {
                if (_previousMode)
                {
                    //If we previously did cull and now don't we need to set all the submeshes to render
                    for (int index1 = 0; index1 < Index; index1++)
                    {
                        MaterialLibrary matLib = MaterialLib[index1];
                        for (int i = 0; i < matLib.Index; i++)
                        {
                            MeshLibrary meshLib = matLib.GetMeshLibrary()[i];
                            for (int j = 0; j < meshLib.Rendered.Length; j++)
                            {
                                meshLib.Rendered[j] = _previousMode;
                            }
                        }
                    }

                }
                _previousMode = GameSettings.g_cpuculling;

            }

            if (!GameSettings.g_cpuculling) return false;

            bool hasAnythingChanged = false;
            //Ok we applied the transformation to all the entities, now update the submesh boundingboxes!
            // Parallel.For(0, Index, index1 =>
            for (int index1 = 0; index1 < Index; index1++)
            {
                float distance = 0;
                int counter = 0;


                MaterialLibrary matLib = GameSettings.g_cpusort
                    ? MaterialLib[MaterialLibPointer[index1]]
                    : MaterialLib[index1];
                for (int i = 0; i < matLib.Index; i++)
                {
                    MeshLibrary meshLib = matLib.GetMeshLibrary()[i];
                    float? distanceSq = meshLib.UpdatePositionAndCheckRender(hasCameraChanged, boundingFrustrum,
                        cameraPosition, _defaultBoundingSphere);

                    //If we get a new distance, apply it to the material
                    if (distanceSq != null)
                    {
                        distance += (float)distanceSq;
                        counter++;
                        hasAnythingChanged = true;
                    }
                }

                if (Math.Abs(distance) > 0.00001f)
                {
                    distance /= counter;
                    matLib.DistanceSquared = distance;
                    matLib.HasChangedThisFrame = true;
                }
            }//);

            //finally sort the materials by distance. Bubble sort should in theory be fast here since little changes.
            if (hasAnythingChanged)
                SortByDistance();

            return hasAnythingChanged;
        }


        public void FlagMovedObjects(List<ModelRenderer> entities)
        {
            //if (_previousEditorMode != GameSettings.e_enableeditor)
            //{
            //    _previousEditorMode = GameSettings.e_enableeditor;
            //    //Set Changed to true
            //    for (int index1 = 0; index1 < entities.Count; index1++)
            //    {
            //        BasicEntity entity = entities[index1];
            //        entity.WorldTransform.HasChanged = true;
            //    }

            //    for (int index1 = 0; index1 < Index; index1++)
            //    {
            //        MaterialLibrary matLib = GameSettings.g_cpusort ? MaterialLib[MaterialLibPointer[index1]] : MaterialLib[index1];

            //        matLib.HasChangedThisFrame = true;
            //    }
            //}

            for (int index1 = 0; index1 < entities.Count; index1++)
            {
                ModelRenderer entity = entities[index1];
                //entity.CheckPhysics();
            }

        }

        /// <summary>
        /// Should be called when the frame is done.
        /// </summary>
        /// <param name="entities"></param>
        public void FrustumCullingFinalizeFrame(List<ModelRenderer> entities)
        {

            //Set Changed to false
            for (int index1 = 0; index1 < entities.Count; index1++)
            {
                ModelRenderer entity = entities[index1];
                entity.ModelDefinition.Transform.HasChanged = false;
            }

            for (int index1 = 0; index1 < Index; index1++)
            {
                MaterialLibrary matLib = GameSettings.g_cpusort ? MaterialLib[MaterialLibPointer[index1]] : MaterialLib[index1];

                matLib.HasChangedThisFrame = false;
            }

        }

        public void Draw(RenderType renderType, Matrix viewProjection, bool lightViewPointChanged = false, bool hasAnyObjectMoved = false, bool outlined = false, int outlineId = 0, Matrix? view = null, IRenderModule renderModule = null)
        {
            SetBlendAndRasterizerState(renderType);

            if (renderType == RenderType.ShadowLinear || renderType == RenderType.ShadowOmnidirectional)
            {
                //For shadowmaps we need to find out whether any object has moved and if so if it is rendered. If yes, redraw the whole frame, if no don't do anything
                if (!CheckShadowMapUpdateNeeds(lightViewPointChanged, hasAnyObjectMoved))
                    return;
            }

            for (int index1 = 0; index1 < Index; index1++)
            {
                MaterialLibrary matLib = MaterialLib[index1];

                if (matLib.Index < 1) continue;

                //if none of this materialtype is drawn continue too!
                bool isUsed = false;

                for (int i = 0; i < matLib.Index; i++)
                {
                    MeshLibrary meshLib = matLib.GetMeshLibrary()[i];

                    //If it's set to "not rendered" skip
                    for (int j = 0; j < meshLib.Rendered.Length; j++)
                    {
                        if (meshLib.Rendered[j])
                        {
                            isUsed = true;
                            //if (meshLib.GetWorldMatrices()[j].HasChanged)
                            //    hasAnyObjectMoved = true;
                        }

                        if (isUsed)// && hasAnyObjectMoved)
                            break;

                    }
                }

                if (!isUsed) continue;

                //Count the draws of different materials!

                MaterialEffect material = matLib.GetMaterial();

                //Check if alpha or opaque!
                if (renderType == RenderType.Opaque && material.IsTransparent || renderType == RenderType.Opaque && material.Type == MaterialEffect.MaterialTypes.ForwardShaded)
                    continue;
                if (renderType == RenderType.Hologram && material.Type != MaterialEffect.MaterialTypes.Hologram)
                    continue;
                if (renderType != RenderType.Hologram && material.Type == MaterialEffect.MaterialTypes.Hologram)
                    continue;

                if (renderType == RenderType.SubsurfaceScattering && material.Type != MaterialEffect.MaterialTypes.SubsurfaceScattering)
                    continue;

                if (renderType == RenderType.Forward && material.Type != MaterialEffect.MaterialTypes.ForwardShaded)
                    continue;

                //Set the appropriate Shader for the material
                if (renderType == RenderType.ShadowOmnidirectional || renderType == RenderType.ShadowLinear)
                {
                    if (!material.HasShadow)
                        continue;
                }

                if (renderType != RenderType.IdRender && renderType != RenderType.IdOutline)
                    GameStats.MaterialDraws++;

                // TODO: PerMaterialSettings(renderType, material, renderModule);

                for (int i = 0; i < matLib.Index; i++)
                {
                    MeshLibrary meshLib = matLib.GetMeshLibrary()[i];

                    //Initialize the mesh VB and IB
                    graphicsDevice.SetVertexBuffer(meshLib.GetMesh().VertexBuffer);
                    graphicsDevice.Indices = (meshLib.GetMesh().IndexBuffer);
                    int primitiveCount = meshLib.GetMesh().PrimitiveCount;
                    int vertexOffset = meshLib.GetMesh().VertexOffset;
                    //int vCount = meshLib.GetMesh().NumVertices;
                    int startIndex = meshLib.GetMesh().StartIndex;

                    //Now draw the local meshes!
                    for (int index = 0; index < meshLib.Index; index++)
                    {

                        //If it's set to "not rendered" skip
                        //if (!meshLib.GetWorldMatrices()[index].Rendered) continue;
                        if (!meshLib.Rendered[index]) continue;

                        Matrix localWorldMatrix = meshLib.GetWorldMatrices()[index].WorldTransformation.ToMonoGameMatrix();

                        if (!ApplyShaders(renderType, renderModule, localWorldMatrix, view, viewProjection, meshLib, index,
                                outlineId, outlined)) continue;
                        GameStats.MeshDraws++;

                        graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, vertexOffset, startIndex,
                                primitiveCount);
                    }
                }

                //Reset to 
                if (material.RenderCClockwise)
                    graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            }
        }

        /// <summary>
        /// Checks whether or not anything has moved, if no then we ignore the frame
        /// </summary>
        /// <param name="lightViewPointChanged"></param>
        /// <param name="hasAnyObjectMoved"></param>
        /// <returns></returns>
        private bool CheckShadowMapUpdateNeeds(bool lightViewPointChanged, bool hasAnyObjectMoved)
        {
            if (lightViewPointChanged || hasAnyObjectMoved)
            {
                bool discardFrame = true;

                for (int index1 = 0; index1 < Index; index1++)
                {
                    MaterialLibrary matLib = MaterialLib[index1];

                    //We determined beforehand whether something changed this frame
                    if (matLib.HasChangedThisFrame)
                    {
                        for (int i = 0; i < matLib.Index; i++)
                        {
                            //Now we have to check whether we have a rendered thing in here
                            MeshLibrary meshLib = matLib.GetMeshLibrary()[i];
                            for (int index = 0; index < meshLib.Index; index++)
                            {
                                //If it's set to "not rendered" skip
                                for (int j = 0; j < meshLib.Rendered.Length; j++)
                                {
                                    if (meshLib.Rendered[j])
                                    {
                                        discardFrame = false;
                                        break;
                                    }
                                }

                                if (!discardFrame) break;

                            }
                        }
                        if (!discardFrame) break;
                    }
                }

                if (discardFrame) return false;

                graphicsDevice.DepthStencilState = _depthWrite;
                ClearFrame(graphicsDevice);
                graphicsDevice.DepthStencilState = DepthStencilState.Default;

            }

            GameStats.activeShadowMaps++;

            return true;
        }

        private void SetBlendAndRasterizerState(RenderType renderType)
        {
            //Default, Opaque!
            if (renderType != RenderType.Forward)
            {
                if (renderType != RenderType.ShadowOmnidirectional)
                {
                    graphicsDevice.BlendState = BlendState.Opaque;
                    graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
                }
                else //Need special rasterization
                {
                    graphicsDevice.DepthStencilState = DepthStencilState.Default;
                    graphicsDevice.BlendState = BlendState.Opaque;
                    graphicsDevice.RasterizerState = _shadowGenerationRasterizerState;
                }
            }
            else //if (renderType == RenderType.alpha)
            {
                graphicsDevice.BlendState = BlendState.NonPremultiplied;
                graphicsDevice.DepthStencilState = DepthStencilState.Default;
                graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            }
        }

        private bool ApplyShaders(RenderType renderType, IRenderModule renderModule, Matrix localWorldMatrix, Matrix? view, Matrix viewProjection, MeshLibrary meshLib, int index, int outlineId, bool outlined)
        {
            if (renderType == RenderType.Opaque
                || renderType == RenderType.ShadowLinear
                || renderType == RenderType.ShadowOmnidirectional
                || renderType == RenderType.SubsurfaceScattering
                || renderType == RenderType.Forward)
            {
                renderModule.Apply(localWorldMatrix, view, viewProjection);
            }
            // TODO: Other render types.

            return true;
        }

        private void ClearFrame(GraphicsDevice graphicsDevice)
        {
            Shaders.DeferredClear.CurrentTechnique.Passes[0].Apply();
            _fullScreenTriangle.Draw(graphicsDevice);
        }

        public enum RenderType
        {
            Opaque,
            ShadowOmnidirectional,
            ShadowLinear,
            Hologram,
            IdRender,
            IdOutline,
            SubsurfaceScattering,
            Forward,
        }

        public class MaterialLibrary
        {
            private MaterialEffect _material;

            //Determines how many different meshes we have per texture. 
            const int InitialLibrarySize = 2;
            private MeshLibrary[] _meshLib = new MeshLibrary[InitialLibrarySize];
            public int Index;

            //efficiency: REnder front to back. So we must know the distance!

            public float DistanceSquared;
            public bool HasChangedThisFrame = true;

            public void SetMaterial(ref MaterialEffect mat)
            {
                _material = mat;
            }

            public bool HasMaterial(MaterialEffect mat)
            {
                if (!GameSettings.g_batchbymaterial) return false;
                return mat.Equals(_material);
            }

            public MaterialEffect GetMaterial()
            {
                return _material;
            }

            public MeshLibrary[] GetMeshLibrary()
            {
                return _meshLib;
            }

            public void Register(ModelMeshPart mesh, Transform worldMatrix, BoundingSphere boundingSphere)
            {
                bool found = false;
                //Check if we already have a model like that, if yes put it in there!
                for (var i = 0; i < Index; i++)
                {
                    MeshLibrary meshLib = _meshLib[i];
                    if (meshLib.HasMesh(mesh))
                    {
                        meshLib.Register(worldMatrix);
                        found = true;
                        break;
                    }
                }

                //We have no Lib yet, make a new one.
                if (!found)
                {
                    _meshLib[Index] = new MeshLibrary();
                    _meshLib[Index].SetMesh(mesh);
                    _meshLib[Index].SetBoundingSphere(boundingSphere);
                    _meshLib[Index].Register(worldMatrix);
                    Index++;
                }

                //If we exceeded our array length, make the array bigger.
                if (Index >= _meshLib.Length)
                {
                    MeshLibrary[] tempLib = new MeshLibrary[Index + 1];
                    _meshLib.CopyTo(tempLib, 0);
                    _meshLib = tempLib;
                }
            }

            public bool DeleteFromRegistry(ModelMeshPart mesh, Transform worldMatrix)
            {
                for (var i = 0; i < Index; i++)
                {
                    MeshLibrary meshLib = _meshLib[i];
                    if (meshLib.HasMesh(mesh))
                    {
                        if (meshLib.DeleteFromRegistry(worldMatrix)) //if true, we can delete it from registry
                        {
                            for (var j = i; j < Index - 1; j++)
                            {
                                //slide down one
                                _meshLib[j] = _meshLib[j + 1];

                            }
                            Index--;
                        }
                        break;
                    }
                }
                if (Index <= 0) return true; //this material is no longer needed.
                return false;
            }
        }

        public class MeshLibrary
        {
            private ModelMeshPart _mesh;
            public BoundingSphere MeshBoundingSphere;

            const int InitialLibrarySize = 4;
            private Transform[] _worldMatrices = new Transform[InitialLibrarySize];

            //the local displacement of the boundingsphere!
            private Vector3[] _worldBoundingCenters = new Vector3[InitialLibrarySize];
            //the local mode - either rendered or not!
            public bool[] Rendered = new bool[InitialLibrarySize];

            public int Index;

            public void SetMesh(ModelMeshPart mesh)
            {
                _mesh = mesh;
            }

            public bool HasMesh(ModelMeshPart mesh)
            {
                return mesh == _mesh;
            }

            public ModelMeshPart GetMesh()
            {
                return _mesh;
            }

            public Transform[] GetWorldMatrices()
            {
                return _worldMatrices;
            }

            public Vector3 GetBoundingCenterWorld(int index)
            {
                return _worldBoundingCenters[index];
            }

            //IF a submesh belongs to an entity that has moved we need to update the BoundingBoxWorld Position!
            //returns the mean distance of all objects iwth that material
            public float? UpdatePositionAndCheckRender(bool cameraHasChanged, BoundingFrustum viewFrustumEx, Vector3 cameraPosition, BoundingSphere sphere)
            {
                float? distance = null;

                bool hasAnythingChanged = false;

                for (var i = 0; i < Index; i++)
                {
                    Transform trafoMatrix = _worldMatrices[i];


                    if (trafoMatrix.HasChanged) {
                        Vector3 center = MeshBoundingSphere.Center;
                        System.Numerics.Vector3 numericsCenter = trafoMatrix.TransformMatrixSubModel(new System.Numerics.Vector3(center.X, center.Y, center.Z));
                        _worldBoundingCenters[i] = new Vector3(numericsCenter.X, numericsCenter.Y, numericsCenter.Z);
                    }

                    //If either the trafomatrix or the camera has changed we need to check visibility
                    if (trafoMatrix.HasChanged || cameraHasChanged)
                    {
                        sphere.Center = _worldBoundingCenters[i];
                        sphere.Radius = MeshBoundingSphere.Radius * trafoMatrix.Scale.X;
                        if (viewFrustumEx.Contains(sphere) == ContainmentType.Disjoint)
                        {
                            Rendered[i] = false;
                        }
                        else
                        {
                            Rendered[i] = true;
                        }

                        //we just register that something has changed
                        hasAnythingChanged = true;
                    }

                }

                //We need to calcualte a new average distance
                if (hasAnythingChanged && GameSettings.g_cpusort)
                {
                    distance = 0;

                    for (var i = 0; i < Index; i++)
                    {
                        distance += Vector3.DistanceSquared(cameraPosition, _worldBoundingCenters[i]);
                    }
                }

                return distance;
            }

            //Basically no chance we have the same model already. We should be fine just adding it to the list if we did everything else right.
            public void Register(Transform world)
            {
                _worldMatrices[Index] = world;
                Rendered[Index] = true;

                Vector3 center = MeshBoundingSphere.Center;
                System.Numerics.Vector3 numericsCenter = world.TransformMatrixSubModel(new System.Numerics.Vector3(center.X, center.Y, center.Z));

                _worldBoundingCenters[Index] = new Vector3(numericsCenter.X, numericsCenter.Y, numericsCenter.Z);

                Index++;

                //mesh.Effect = Shaders.AmbientEffect; //Just so it has few properties!

                if (Index >= _worldMatrices.Length)
                {
                    Transform[] tempLib = new Transform[Index + 1];
                    _worldMatrices.CopyTo(tempLib, 0);
                    _worldMatrices = tempLib;

                    Vector3[] tempLib2 = new Vector3[Index + 1];
                    _worldBoundingCenters.CopyTo(tempLib2, 0);
                    _worldBoundingCenters = tempLib2;

                    bool[] tempRendered = new bool[Index + 1];
                    Rendered.CopyTo(tempRendered, 0);
                    Rendered = tempRendered;
                }
            }

            public bool DeleteFromRegistry(Transform worldMatrix)
            {
                for (var i = 0; i < Index; i++)
                {
                    Transform trafoMatrix = _worldMatrices[i];

                    if (trafoMatrix == worldMatrix)
                    {
                        //delete this value!
                        for (var j = i; j < Index - 1; j++)
                        {
                            //slide down one
                            _worldMatrices[j] = _worldMatrices[j + 1];
                            Rendered[j] = Rendered[j + 1];
                            _worldBoundingCenters[j] = _worldBoundingCenters[j + 1];
                        }
                        Index--;
                        break;
                    }
                }
                if (Index <= 0) return true; //this meshtype no longer needed!
                return false;
            }

            public void SetBoundingSphere(BoundingSphere boundingSphere)
            {
                MeshBoundingSphere = boundingSphere;
            }
        }

    }

    public class FullScreenTriangle
    {
        private VertexBuffer vertexBuffer;

        public struct FullScreenQuadVertex
        {
            // Stores the starting position of the particle.
            public Vector2 Position;

            public static readonly VertexDeclaration VertexDeclaration = new VertexDeclaration
            (
                new VertexElement(0, VertexElementFormat.Vector2,
                    VertexElementUsage.Position, 0)
            );

            public FullScreenQuadVertex(Vector2 position)
            {
                Position = position;
            }

            public const int SizeInBytes = 8;
        }


        //No function in monogame to draw without vb it seems...

        /*
         struct VertexShaderInput
        {
            float2 Position : POSITION0;
        };

        VertexShaderOutput VertexShaderFunction(VertexShaderInput input, uint id:SV_VERTEXID)
        {
        VertexShaderOutput output;
        output.Position = float4(input.Position, 0, 1);
        output.TexCoord.x = (float)(id / 2) * 2.0;
        output.TexCoord.y = 1.0 - (float)(id % 2) * 2.0;

        return output;
        }

        */

        public FullScreenTriangle(GraphicsDevice graphics)
        {
            FullScreenQuadVertex[] vertices = new FullScreenQuadVertex[3];
            vertices[0] = new FullScreenQuadVertex(new Vector2(-1, -1));
            vertices[1] = new FullScreenQuadVertex(new Vector2(-1, 3));
            vertices[2] = new FullScreenQuadVertex(new Vector2(3, -1));

            vertexBuffer = new VertexBuffer(graphics, FullScreenQuadVertex.VertexDeclaration, 3, BufferUsage.WriteOnly);
            vertexBuffer.SetData(vertices);
        }

        public void Draw(GraphicsDevice graphics)
        {
            graphics.SetVertexBuffer(vertexBuffer);
            graphics.Indices = null;

            graphics.DrawPrimitives(PrimitiveType.TriangleList, 0, 1);
        }

        public void Dispose()
        {
            vertexBuffer?.Dispose();
        }
    }
}
