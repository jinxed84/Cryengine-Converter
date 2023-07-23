﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using CgfConverter.CryEngineCore;
using CgfConverter.Renderers.Gltf.Models;
using Extensions;

namespace CgfConverter.Renderers.Gltf;

public partial class BaseGltfRenderer
{
    // Crynode should always be from model[0]
    protected int CreateGltfNode(ChunkNode cryNode, bool omitSkins = false)
    {
        var controllerIdToNodeIndex = new Dictionary<uint, int>();

        // Create this node and add to GltfRoot.Nodes
        var rotationQuat = Quaternion.CreateFromRotationMatrix(cryNode.LocalTransform);
        var swappedAxesQuat = SwapAxesForLayout(rotationQuat);
        var node = new GltfNode()
        {
            Name = cryNode.Name,
            Rotation = SwapAxesForLayout(rotationQuat).ToGltfList(),
            Translation = SwapAxesForPosition(cryNode.LocalTransform.GetTranslation()).ToGltfList(),
            Scale = Vector3.One.ToGltfList()
        };
        _gltfRoot.Nodes.Add(node);
        var nodeIndex = _gltfRoot.Nodes.Count - 1;

        // Add mesh if needed
        if (_cryData.Models[0].IsIvoFile || 
            _cryData.Models[0].ChunkMap[cryNode.ObjectNodeID].ChunkType != ChunkType.Helper)
        {
            if (_cryData.Models.Count == 1)
                AddMesh(cryNode, node, controllerIdToNodeIndex, omitSkins);
            else  // Has geometry file
            {
                string nodeName = cryNode.Name;
                int nodeId = cryNode.ID;

                // Some nodes don't have matching geometry in geometry file, even though the object chunk for the node
                // points to a mesh chunk ($PHYSICS_Proxy_Tail in Buccaneer Blue).  Check if the node exists in the geometry
                // file, and if not, continue processing.
                ChunkNode geometryNode = _cryData.Models[1].NodeMap.Values.Where(a => a.Name == cryNode.Name).FirstOrDefault();
                if (geometryNode is not null)
                {
                    ChunkMesh geometryMesh = (ChunkMesh)_cryData.Models[1].ChunkMap[geometryNode.ObjectNodeID];
                    if (geometryMesh is not null && geometryMesh.NumIndices != 0)
                        AddMesh(geometryNode, node, controllerIdToNodeIndex, omitSkins);
                }
            }
        }

        // For each child, recursively call this method to add the child to GltfRoot.Nodes.
        if (cryNode.AllChildNodes is not null)
        {
            foreach (var child in cryNode.AllChildNodes)
            {
                node.Children.Add(CreateGltfNode(child));
            }
        }

        // Returns the int for the index of this node.
        return nodeIndex;
    }

    private void AddMesh(ChunkNode cryNode, GltfNode gltfNode, Dictionary<uint, int> controllerIdToNodeIndex, bool omitSkins = false)
    {
        var accessors = new GltfMeshPrimitiveAttributes();
        var meshChunk = cryNode.ObjectChunk as ChunkMesh;
        WriteMeshOrLogError(out var gltfMesh, gltfNode, cryNode, meshChunk!, accessors);

        if (omitSkins)
            Log.D("NodeChunk[0]: Skipping skins.", cryNode.Name);
        else if (cryNode.GetSkinningInfo() is { HasSkinningInfo: true } skinningInfo)
        {
            if (WriteSkinOrLogError(out var newSkin, out var weights, out var joints, gltfNode, skinningInfo,
                    controllerIdToNodeIndex))
            {
                gltfNode.Skin = AddSkin(newSkin);
                foreach (var prim in gltfMesh.Primitives)
                {
                    prim.Attributes.Joints0 = joints;
                    prim.Attributes.Weights0 = weights;
                }
            }
        }
        else
            Log.D("NodeChunk[{0}]: No skinning info is available.", cryNode.Name);

        gltfNode.Mesh = AddMesh(gltfMesh);
    }

    // Currently used for terrain
    protected bool CreateModelNode(out GltfNode node, CryEngine cryObject, bool omitSkins = false)
    {
        var model = cryObject.Models[^1];
        var controllerIdToNodeIndex = new Dictionary<uint, int>();

        var rootNodeName = Path.GetFileNameWithoutExtension(model.FileName!);
        var childNodes = new List<GltfNode>();

        // THERE CAN BE MULTIPLE ROOT NODES IN EACH FILE!  Check to see if the parentnodeid ~0 and be sure to add a node for it.
        foreach (var nodeChunk in model.ChunkMap.Values.OfType<ChunkNode>())
        {
            if (Args.IsNodeNameExcluded(nodeChunk.Name))
            {
                Log.D("NodeChunk[{0}]: Excluded.", nodeChunk.Name);
                continue;
            }

            // this isn't right.  need to add proxy nodes too.
            if (nodeChunk.ObjectChunk is not ChunkMesh meshChunk)
            {
                Log.D("NodeChunk[{0}]: Skipped; no valid ChunkMesh is referenced to.", nodeChunk.Name);
                continue;
            }

            var rootNode = new GltfNode
            {
                Name = nodeChunk.Name,
                Rotation = Quaternion.CreateFromRotationMatrix(nodeChunk.LocalTransform).ToGltfList(),
                Translation = nodeChunk.LocalTransform.GetTranslation().ToGltfList(true),
                Scale = nodeChunk.Scale.ToGltfList(),
            };

            var accessors = new GltfMeshPrimitiveAttributes();

            if (!WriteMeshOrLogError(out var newMesh, rootNode, nodeChunk, meshChunk, accessors))
                continue;

            if (omitSkins)
                Log.D("NodeChunk[0]: Skipping skins.", nodeChunk.Name);
            else if (nodeChunk.GetSkinningInfo() is { HasSkinningInfo: true } skinningInfo)
            {
                if (WriteSkinOrLogError(out var newSkin, out var weights, out var joints, rootNode, skinningInfo,
                        controllerIdToNodeIndex))
                {
                    rootNode.Skin = AddSkin(newSkin);
                    foreach (var prim in newMesh.Primitives)
                    {
                        prim.Attributes.Joints0 = joints;
                        prim.Attributes.Weights0 = weights;
                    }
                }
            }
            else
                Log.D("NodeChunk[{0}]: No skinning info is available.", nodeChunk.Name);

            rootNode.Mesh = AddMesh(newMesh);
            childNodes.Add(rootNode);
        }

        if (omitSkins)
            Log.D("Model[{0}]: Skipping animations.", rootNodeName);
        else
        {
            var numAnimations = WriteAnimations(cryObject.Animations, controllerIdToNodeIndex);
            if (numAnimations == 0)
                Log.D("Model[{0}]: No associated animations found.");
            else
                Log.I("Model[{0}]: Written {1} animations.", rootNodeName, numAnimations);
        }

        switch (childNodes.Count)
        {
            case 0:
                Log.D("Model[{0}]: Empty.", rootNodeName);
                node = null!;
                return false;
            case 1:
                Log.D("Model[{0}]: Wrote 1 node.", rootNodeName);
                node = childNodes.First();
                return true;
            default:
                Log.D("Model[{0}]: Wrote {1} nodes.", rootNodeName, childNodes.Count);
                node = new GltfNode
                {
                    Name = rootNodeName,
                    Children = childNodes.Select(AddNode).ToList(),
                };
                return true;
        }
    }

    private bool WriteSkinOrLogError(
        out GltfSkin newSkin,
        out int weights,
        out int joints,
        GltfNode rootNode,
        SkinningInfo skinningInfo,
        IDictionary<uint, int> controllerIdToNodeIndex)
    {
        if (!skinningInfo.HasSkinningInfo)
            throw new ArgumentException("HasSkinningInfo must be true", nameof(skinningInfo));

        newSkin = null!;
        weights = joints = 0;

        var baseName = $"{rootNode.Name}/bone/weight";

        weights =
            GetAccessorOrDefault(baseName, 0,
                skinningInfo.IntVertices is null ? skinningInfo.BoneMapping.Count : skinningInfo.Ext2IntMap.Count)
            ?? AddAccessor(baseName, -1, null,
                skinningInfo.IntVertices is null
                    ? skinningInfo.BoneMapping
                        .Select(x => new TypedVec4<float>(
                            x.Weight[0] / 255f, x.Weight[1] / 255f, x.Weight[2] / 255f, x.Weight[3] / 255f))
                        .ToArray()
                    : skinningInfo.Ext2IntMap
                        .Select(x => skinningInfo.IntVertices[x])
                        .Select(x => new TypedVec4<float>(
                            x.Weights[0], x.Weights[1], x.Weights[2], x.Weights[3]))
                        .ToArray());

        var boneIdToBindPoseMatrices = new Dictionary<uint, Matrix4x4>();
        foreach (var bone in skinningInfo.CompiledBones)
        {
            var mat = boneIdToBindPoseMatrices[bone.ControllerID] = bone.BindPoseMatrix;
            if (bone.parentID != 0)
            {
                if (!Matrix4x4.Invert(boneIdToBindPoseMatrices[bone.parentID], out var parentMat))
                    return Log.E<bool>("CompiledBone[{0}/{1}]: Failed to invert BindPoseMatrix.",
                        rootNode.Name, bone.ParentBone?.boneName);

                mat *= parentMat;
            }

            if (!Matrix4x4.Invert(mat, out mat))
                return Log.E<bool>("CompiledBone[{0}/{1}]: Failed to invert BindPoseMatrix.",
                    rootNode.Name, bone.boneName);

            mat = SwapAxes(Matrix4x4.Transpose(mat));
            if (!Matrix4x4.Decompose(mat, out var scale, out var rotation, out var translation))
                return Log.E<bool>("CompiledBone[{0}/{1}]: BindPoseMatrix is not decomposable.",
                    rootNode.Name, bone.boneName);

            controllerIdToNodeIndex[bone.ControllerID] = AddNode(new GltfNode
            {
                Name = bone.boneName,
                Scale = (scale - Vector3.One).LengthSquared() > 0.000001
                    ? new List<float> { scale.X, scale.Y, scale.Z }
                    : null,
                Translation = translation != Vector3.Zero
                    ? new List<float> { translation.X, translation.Y, translation.Z }
                    : null,
                Rotation = rotation != Quaternion.Identity
                    ? new List<float> { rotation.X, rotation.Y, rotation.Z, rotation.W }
                    : null,
            });

            if (bone.parentID == 0)
                rootNode.Children.Add(controllerIdToNodeIndex[bone.ControllerID]);
            else
                _gltfRoot.Nodes[controllerIdToNodeIndex[bone.parentID]].Children
                    .Add(controllerIdToNodeIndex[bone.ControllerID]);
        }

        baseName = $"{rootNode.Name}/bone/joint";
        joints =
            GetAccessorOrDefault(baseName, 0,
                skinningInfo.IntVertices is null ? skinningInfo.BoneMapping.Count : skinningInfo.Ext2IntMap.Count)
            ?? AddAccessor(baseName, -1, null,
                skinningInfo is { HasIntToExtMapping: true, IntVertices: { } }
                    ? skinningInfo.Ext2IntMap
                        .Select(x => skinningInfo.IntVertices[x])
                        .Select(x => new TypedVec4<ushort>(
                            x.BoneIDs[0], x.BoneIDs[1], x.BoneIDs[2], x.BoneIDs[3]))
                        .ToArray()
                    : skinningInfo.BoneMapping
                        .Select(x => new TypedVec4<ushort>(
                            (ushort)x.BoneIndex[0], (ushort)x.BoneIndex[1], (ushort)x.BoneIndex[2],
                            (ushort)x.BoneIndex[3]))
                        .ToArray());

        baseName = $"{rootNode.Name}/inverseBindMatrix";
        var inverseBindMatricesAccessor =
            GetAccessorOrDefault(baseName, 0, skinningInfo.CompiledBones.Count)
            ?? AddAccessor(baseName, -1, null,
                skinningInfo.CompiledBones.Select(x => SwapAxes(Matrix4x4.Transpose(x.BindPoseMatrix))).ToArray());

        newSkin = new GltfSkin
        {
            InverseBindMatrices = inverseBindMatricesAccessor,
            Joints = skinningInfo.CompiledBones.Select(x => controllerIdToNodeIndex[x.ControllerID]).ToList(),
            Name = $"{rootNode.Name}/skin",
        };
        return true;
    }

    private bool WriteMeshOrLogError(out GltfMesh newMesh, 
        GltfNode gltfNode, 
        ChunkNode nodeChunk, 
        ChunkMesh mesh,
        GltfMeshPrimitiveAttributes accessors)
    {
        newMesh = null!;

        var vertices = nodeChunk._model.ChunkMap.GetValueOrDefault(mesh.VerticesData) as ChunkDataStream;
        var vertsUvs = nodeChunk._model.ChunkMap.GetValueOrDefault(mesh.VertsUVsData) as ChunkDataStream;
        var normals = nodeChunk._model.ChunkMap.GetValueOrDefault(mesh.NormalsData) as ChunkDataStream;
        var uvs = nodeChunk._model.ChunkMap.GetValueOrDefault(mesh.UVsData) as ChunkDataStream;
        var indices = nodeChunk._model.ChunkMap.GetValueOrDefault(mesh.IndicesData) as ChunkDataStream;
        var colors = nodeChunk._model.ChunkMap.GetValueOrDefault(mesh.ColorsData) as ChunkDataStream;
        var colors2 = nodeChunk._model.ChunkMap.GetValueOrDefault(mesh.Colors2Data) as ChunkDataStream;
        var tangents = nodeChunk._model.ChunkMap.GetValueOrDefault(mesh.TangentsData) as ChunkDataStream;
        var subsets = nodeChunk._model.ChunkMap.GetValueOrDefault(mesh.MeshSubsetsData) as ChunkMeshSubsets;

        if (indices is null)
            return Log.D<bool>("Mesh[{0}]: IndicesData is empty.", gltfNode.Name);
        if (subsets is null)
            return Log.D<bool>("Mesh[{0}]: MeshSubsetsData is empty.", gltfNode.Name);
        if (vertices is null && vertsUvs is null)
            return Log.D<bool>("Mesh[{0}]: both VerticesData and VertsUVsData are empty.", gltfNode.Name);

        var materialMap = WriteMaterial(nodeChunk);
        if (subsets.MeshSubsets
                .Select(x => materialMap.GetValueOrDefault(x.MatID))
                .All(x => x?.IsSkippedFromArgs ?? false))
            return false;

        var usesTangent = subsets.MeshSubsets.Any(v =>
            materialMap.GetValueOrDefault(v.MatID) is { } m && m.Target?.HasNormalTexture() is true);

        var usesUv = usesTangent || subsets.MeshSubsets.Any(v =>
            materialMap.GetValueOrDefault(v.MatID) is { } m && m.Target?.HasAnyTexture() is true);

        string baseName;

        if (vertices is not null || vertsUvs is not null)
        {
            if (vertices is not null)
            {
                baseName = $"{gltfNode.Name}/vertex";
                accessors.Position =
                    GetAccessorOrDefault(baseName, 0, vertices.Vertices.Length)
                    ?? AddAccessor(baseName, -1, GltfBufferViewTarget.ArrayBuffer,
                        vertices.Vertices.Select(SwapAxesForPosition).ToArray());

                baseName = $"${gltfNode.Name}/uv";
                accessors.TexCoord0 =
                    uvs is null
                        ? null
                        : GetAccessorOrDefault(baseName, 0, uvs.UVs.Length)
                          ?? AddAccessor($"{nodeChunk.Name}/uv", -1, GltfBufferViewTarget.ArrayBuffer, uvs.UVs);
            }
            else  // VertsUVs.
            {
                baseName = $"{gltfNode.Name}/vertex";
                var multiplerVector = Vector3.Abs((mesh.MinBound - mesh.MaxBound) / 2f);
                if (multiplerVector.X < 1) { multiplerVector.X = 1; }
                if (multiplerVector.Y < 1) { multiplerVector.Y = 1; }
                if (multiplerVector.Z < 1) { multiplerVector.Z = 1; }
                var boundaryBoxCenter = (mesh.MinBound + mesh.MaxBound) / 2f;
                var scaleToBBox = _cryData.InputFile.EndsWith("cga") || _cryData.InputFile.EndsWith("cgf");

                accessors.Position =
                    GetAccessorOrDefault(baseName, 0, vertsUvs.Vertices.Length)
                        ?? AddAccessor(
                            baseName, 
                            -1, 
                            GltfBufferViewTarget.ArrayBuffer,
                            vertsUvs.Vertices.Select(x => scaleToBBox ? SwapAxesForPosition((x * multiplerVector) + boundaryBoxCenter) : SwapAxesForPosition(x)).ToArray());
                baseName = $"${gltfNode.Name}/uv";
                accessors.TexCoord0 =
                    GetAccessorOrDefault(baseName, 0, vertsUvs.UVs.Length)
                        ?? AddAccessor(
                            $"{nodeChunk.Name}/uv", 
                            -1, 
                            GltfBufferViewTarget.ArrayBuffer, 
                            vertsUvs.UVs);
            }

            var normalsArray = normals?.Normals ?? tangents?.Normals;
            baseName = $"{gltfNode.Name}/normal";
            accessors.Normal = normalsArray is null
                ? null
                : GetAccessorOrDefault(baseName, 0, normalsArray.Length)
                  ?? AddAccessor(baseName, -1, GltfBufferViewTarget.ArrayBuffer,
                      normalsArray.Select(SwapAxesForPosition).ToArray());

            // TODO: Is this correct? This breaks some of RoL model colors, while having it set does not make anything better.
            baseName = $"{gltfNode.Name}/colors";
            accessors.Color0 = colors is null
                ? null
                : (GetAccessorOrDefault(baseName, 0, colors.Colors.Length)
                    ?? AddAccessor(
                        baseName, 
                        -1, 
                        GltfBufferViewTarget.ArrayBuffer,
                        colors.Colors.Select(x => new TypedVec4<float>(x.r / 255f, x.g / 255f, x.b / 255f, x.a / 255f))
                            .ToArray()));

            // TODO: Do Tangents also need swapping axes?
            baseName = $"${gltfNode.Name}/tangent";
            accessors.Tangent = tangents is null || !usesTangent
                ? null
                : GetAccessorOrDefault(baseName, 0, tangents.Tangents.Length / 2)
                  ?? AddAccessor(baseName, -1, GltfBufferViewTarget.ArrayBuffer,
                      tangents.Tangents.Cast<Tangent>()
                          .Where((_, i) => i % 2 == 0)
                          .Select(x => new TypedVec4<float>(x.x / 32767f, x.y / 32767f, x.z / 32767f, x.w / 32767f))
                          .ToArray());

        }

        //if (vertsUvs is not null && vertices is null)
        //    return Log.E<bool>("Mesh[{0}]: vertsUvs is currently not supported.", gltfNode.Name);  // TODO: Support VertsUvs.

        baseName = $"${gltfNode.Name}/index";
        var indexBufferView = GetBufferViewOrDefault(baseName) ??
                              AddBufferView(baseName, indices.Indices, GltfBufferViewTarget.ElementArrayBuffer);
        
        newMesh = new GltfMesh
        {
            Name = $"{gltfNode.Name}/mesh",
            Primitives = subsets.MeshSubsets
                .Select(x => Tuple.Create(x, materialMap.GetValueOrDefault(x.MatID)))
                .Where(x => !(x.Item2?.IsSkippedFromArgs ?? false))
                .Select(x =>
                {
                    var (v, mat) = x;

                    return new GltfMeshPrimitive
                    {
                        Attributes = new GltfMeshPrimitiveAttributes
                        {
                            Position = accessors.Position,
                            Normal = accessors.Normal,
                            Tangent = mat?.Target?.HasNormalTexture() is true ? accessors.Tangent : null,
                            TexCoord0 = mat?.Target?.HasAnyTexture() is true ? accessors.TexCoord0 : null,
                            Color0 = accessors.Color0,
                        },
                        Indices = GetAccessorOrDefault(baseName, v.FirstIndex, v.FirstIndex + v.NumIndices)
                                  ?? AddAccessor(
                                      $"{nodeChunk.Name}/index",
                                      indexBufferView, GltfBufferViewTarget.ElementArrayBuffer,
                                      indices.Indices, v.FirstIndex, v.FirstIndex + v.NumIndices),
                        Material = mat?.Index,
                    };
                })
                .ToList()
        };

        return newMesh.Primitives.Any();
    }
}
