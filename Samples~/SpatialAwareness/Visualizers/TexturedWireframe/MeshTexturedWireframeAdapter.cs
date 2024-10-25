// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// Copyright (c) (2018-2022) Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Software License Agreement, located here: https://www.magicleap.com/software-license-agreement-ml2
// Terms and conditions applicable to third-party materials accompanying this distribution may also be found in the top-level NOTICE file appearing herein.
// %COPYRIGHT_END%
// ---------------------------------------------------------------------
// %BANNER_END%

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.MagicLeap;
using System.Linq;
using UnityEngine.XR.ARFoundation;

// Somewhat based on the texture-based wireframe technique described in
// http://sibgrapi.sid.inpe.br/col/sid.inpe.br/sibgrapi/2010/09.15.18.18/doc/texture-based_wireframe_rendering.pdf
// See Figure 5c and related description

namespace MagicLeap.MRTK.Samples.SpatialAwareness
{
    /// <summary>
    /// Adapts and prepares meshes from MeshingSubsytemComponent or ARMeshManager, to use the TexturedWireframe material and shader.
    /// </summary>
    public class MeshTexturedWireframeAdapter : MonoBehaviour
    {
        public Material WireframeMaterial
        {
            get { return wireframeMaterial; }
        }

        [SerializeField, Tooltip("The textured wireframe material.")]
        private Material wireframeMaterial = null;

        private MeshingSubsystemComponent meshingSubsystemComponent = null;
        private ARMeshManager meshManager = null;
        private Texture2D proceduralTexture = null;
        private int lineTextureWidth = 2048;       // Overall width of texture used for the line (will be 1px high)
        private int linePixelWidthL = 24;           // Line fill pixel width (left side) representing line, over background
        private int lineEdgeGradientWidth = 4;     // Falloff gradient pixel size to smooth line edge

        void Awake()
        {
            if (wireframeMaterial != null)
            {
                // Create procedural texture used to render the line (more control this way over mip-map levels)
                proceduralTexture = new Texture2D(lineTextureWidth, 1, TextureFormat.ARGB32, 7, true);
                int linePixelWidth = linePixelWidthL - (lineEdgeGradientWidth / 2);
                for (int i = 0; i < lineTextureWidth; i++)
                {
                    var color = i <= linePixelWidth ? Color.white :
                                i > linePixelWidth + lineEdgeGradientWidth ? Color.clear :
                                Color.Lerp(Color.white, Color.clear, (float)(i - linePixelWidth) / (float)lineEdgeGradientWidth);
                    proceduralTexture.SetPixel(i, 0, color);
                }
                proceduralTexture.wrapMode = TextureWrapMode.Clamp;
                proceduralTexture.Apply();

                wireframeMaterial.mainTexture = proceduralTexture;
            }
        }

        void Start()
        {
            // Subscribe to meshing changes
            if (MLDevice.IsOpenXRLoaderActive())
            {
                meshManager = FindAnyObjectByType<ARMeshManager>();
                if (meshManager != null)
                {
                    meshManager.meshesChanged += MeshAddedOrUpdatedOpenXR;
                }
            }
            else if (MLDevice.IsMagicLeapLoaderActive())
            {
                meshingSubsystemComponent = FindAnyObjectByType<MeshingSubsystemComponent>();
                if (meshingSubsystemComponent != null)
                {
                    meshingSubsystemComponent.meshAdded += MeshAddedOrUpdatedMLXR;
                    meshingSubsystemComponent.meshUpdated += MeshAddedOrUpdatedMLXR;
                }
            }
        }

        void OnDestroy()
        {
            if (proceduralTexture != null)
            {
                Destroy(proceduralTexture);
                proceduralTexture = null;
            }

            // Unsubscribe to meshing changes
            if (MLDevice.IsOpenXRLoaderActive())
            {
                if (meshManager != null)
                {
                    meshManager.meshesChanged -= MeshAddedOrUpdatedOpenXR;
                }
            } else if (MLDevice.IsMagicLeapLoaderActive())
            {
                if (meshingSubsystemComponent != null)
                {
                    meshingSubsystemComponent.meshAdded -= MeshAddedOrUpdatedMLXR;
                    meshingSubsystemComponent.meshUpdated -= MeshAddedOrUpdatedMLXR;
                }
            }
        }

        private void MeshAddedOrUpdatedOpenXR(ARMeshesChangedEventArgs args)
        {
            List<MeshFilter> alteredMeshes = args.added.Concat(args.updated).ToList();
            foreach (MeshFilter alteredMesh in alteredMeshes)
            {
                GameObject meshGameObject = null;
                if (meshManager.meshPrefab != null)
                {
                    meshGameObject = meshManager.meshPrefab.gameObject;
                    // Check that the mesh is using the wireframe material before proceeding
                    var meshRenderer = meshGameObject.GetComponent<MeshRenderer>();
                    if (meshRenderer != null && meshRenderer.sharedMaterial != wireframeMaterial)
                    {
                        return;
                    }

                    // Adapt the mesh for the textured wireframe shader.
                    if (alteredMesh != null)
                    {
                        GenerateMesh(alteredMesh);
                    }
                }
            }
        }

        private void MeshAddedOrUpdatedMLXR(UnityEngine.XR.MeshId meshId)
        {
            if (meshingSubsystemComponent.requestedMeshType != MeshingSubsystemComponent.MeshType.Triangles)
            {
                return;
            }

            GameObject meshGameObject = null;
            if (meshingSubsystemComponent.meshIdToGameObjectMap.TryGetValue(meshId, out meshGameObject))
            {
                // Check that the mesh is using the wireframe material before proceeding
                var meshRenderer = meshGameObject.GetComponent<MeshRenderer>();
                if (meshRenderer != null && meshRenderer.sharedMaterial != wireframeMaterial)
                {
                    return;
                }

                // Adapt the mesh for the textured wireframe shader.
                var meshFilter = meshGameObject.GetComponent<MeshFilter>();
                if (meshFilter != null)
                {
                    List<float> confidences = new List<float>();
                    bool validConfidences = meshingSubsystemComponent.requestVertexConfidence &&
                                            meshingSubsystemComponent.TryGetConfidence(meshId, confidences);
                    GenerateMesh(meshFilter, validConfidences, confidences);
                }
            }
        }

        private void GenerateMesh(MeshFilter meshFilter, bool validConfidences = false, List<float> confidences = null)
        {
            var origMesh = meshFilter.mesh;
            List<Vector3> vertices = new List<Vector3>();
            origMesh.GetVertices(vertices);
            List<Vector3> uvs = new List<Vector3>(Enumerable.Repeat(Vector3.forward, vertices.Count));
            List<int> indices = new List<int>();
            origMesh.GetTriangles(indices, 0);

            // Encode confidence in uv.z
            if (validConfidences)
            {
                for (int i = 0; i < uvs.Count; i++)
                {
                    var uv = uvs[i];
                    uv.z = confidences[i];
                    uvs[i] = uv;
                }
            }

            int indicesOrigCount = indices.Count;
            for (int i = 0; i < indicesOrigCount; i += 3)
            {
                var i1 = indices[i];
                var i2 = indices[i + 1];
                var i3 = indices[i + 2];

                var v1 = vertices[i1];
                var v2 = vertices[i2];
                var v3 = vertices[i3];

                var uv1 = uvs[i1];
                var uv2 = uvs[i2];
                var uv3 = uvs[i3];

                // Create a new center vertex of each triangle, adjusting indices and add new triangles
                // Will use Incenter of Triangle (center that is equidistant to edges).
                // This allows the line width to be consistent regardless of triangle size.
                // Also allows line width to be adjusted dynamically.
                // Calculate position of incenter vertex
                var a = Vector3.Distance(v2, v3);
                var b = Vector3.Distance(v1, v3);
                var c = Vector3.Distance(v1, v2);
                var sum = a + b + c;
                var vIntercenter = new Vector3((a * v1.x + b * v2.x + c * v3.x) / sum,
                                               (a * v1.y + b * v2.y + c * v3.y) / sum,
                                               (a * v1.z + b * v2.z + c * v3.z) / sum);
                vertices.Add(vIntercenter);
                int iC = vertices.Count - 1;

                // Distance to edge, or radius of incircle
                var s = sum / 2.0f;
                var r = Mathf.Sqrt(((s - a) * (s - b) * (s - c)) / s);

                // Calculate UV for the incenter vertex for a 1mm target line width
                // Half of each line is rendered on the edges of each triangle, so .001/2 = .0005
                // Can be adjusted in shader to vary line width dynamically.
                float lineWidth = .0005f;
                float segmentPixels = (r / lineWidth) * (float)linePixelWidthL;
                float segmentUV = segmentPixels / (float)lineTextureWidth;

                Vector3 centerUV = Vector3.one * segmentUV;
                centerUV.z = validConfidences ? (a * uv1.z + b * uv2.z + c * uv3.z) / sum : 1;
                uvs.Add(centerUV);

                // Modify triangle to emanate from new center vertex, along with 2 new triangles
                indices[i + 2] = iC;

                indices.Add(i1);
                indices.Add(iC);
                indices.Add(i3);

                indices.Add(i2);
                indices.Add(i3);
                indices.Add(iC);
            }

            var mesh = new Mesh();
            mesh.SetVertices(vertices);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(indices, 0);
            if (meshingSubsystemComponent != null && meshingSubsystemComponent.computeNormals)
            {
                mesh.RecalculateNormals();
            }
            meshFilter.mesh = mesh;
        }
    }
}
