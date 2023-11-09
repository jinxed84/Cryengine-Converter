﻿using CgfConverter;
using CgfConverter.Renderers.Collada;
using CgfConverter.Renderers.Gltf;
using CgfConverterIntegrationTests.Extensions;
using CgfConverterTests.TestUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Globalization;
using System.Linq;
using System.Threading;

namespace CgfConverterTests.IntegrationTests;

[TestClass]
public class StarCitizenTests
{
    private readonly TestUtils testUtils = new();
    string userHome;

    [TestInitialize]
    public void Initialize()
    {
        CultureInfo customCulture = (CultureInfo)Thread.CurrentThread.CurrentCulture.Clone();
        customCulture.NumberFormat.NumberDecimalSeparator = ".";
        Thread.CurrentThread.CurrentCulture = customCulture;
        userHome = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        testUtils.GetSchemaSet();
    }

    [TestMethod]
    public void NavyPilotFlightSuit_Ivo()
    {
        var args = new string[] { $@"{userHome}\OneDrive\ResourceFiles\SC\ivo\pilot_flightsuit\m_nvy_pilot_light_helmet_01.skin", "-dds", "-dae", "-objectdir", @"d:\depot\sc2\data" };
        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);
        CryEngine cryData = new(args[0], testUtils.argsHandler.PackFileSystem);
        cryData.ProcessCryengineFiles();

        var colladaData = new ColladaModelRenderer(testUtils.argsHandler, cryData);
        colladaData.GenerateDaeObject();
        var daeObject = colladaData.DaeObject;
    }

    [TestMethod]
    public void CutlassRed_312_NonIvo()
    {
        var args = new string[] { $@"D:\depot\SC2\Data\objects\Spaceships\Ships\DRAK\Cutlass\Cutlass_Red\DRAK_Cutlass_Red.cga", "-dds", "-dae", "-objectdir", @"d:\depot\sc2\data" };
        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);
        CryEngine cryData = new(args[0], testUtils.argsHandler.PackFileSystem);
        cryData.ProcessCryengineFiles();

        var colladaData = new ColladaModelRenderer(testUtils.argsHandler, cryData);
        colladaData.GenerateDaeObject();
        var daeObject = colladaData.DaeObject;
    }

    [TestMethod]
    public void CutlassBlue_312_Gltf_NonIvo()
    {
        var args = new string[] { $@"D:\depot\SC2\Data\objects\Spaceships\Ships\DRAK\Cutlass\Cutlass_Blue\DRAK_Cutlass_Blue.cga", "-dds", "-objectdir", @"d:\depot\sc2\data" };
        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);
        CryEngine cryData = new(args[0], testUtils.argsHandler.PackFileSystem);
        cryData.ProcessCryengineFiles();

        GltfModelRenderer gltfRenderer = new(testUtils.argsHandler, cryData, true, false);
        //var gltfData = gltfRenderer.GenerateGltfObject();
        gltfRenderer.Render();

    }

    [TestMethod]
    public void AEGS_Vanguard_LandingGear_Front_IvoFile()
    {
        var args = new string[] { $@"{userHome}\OneDrive\ResourceFiles\SC\ivo\AEGS_Vanguard_LandingGear_Front.skin", "-dds", "-dae" };

        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);
        CryEngine cryData = new CryEngine(args[0], testUtils.argsHandler.PackFileSystem);
        cryData.ProcessCryengineFiles();

        var colladaData = new ColladaModelRenderer(testUtils.argsHandler, cryData);
        colladaData.GenerateDaeObject();
        var daeObject = colladaData.DaeObject;
    }

    [TestMethod]
    public void M_ccc_vanduul_helmet_01_312IvoSkinFile()
    {
        var args = new string[] { $@"{userHome}\OneDrive\ResourceFiles\SC\ivo\m_ccc_vanduul_helmet_01.skin", "-dds", "-dae" };

        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);
        CryEngine cryData = new(args[0], testUtils.argsHandler.PackFileSystem);
        cryData.ProcessCryengineFiles();

        var colladaData = new ColladaModelRenderer(testUtils.argsHandler, cryData);
        colladaData.GenerateDaeObject();
        var daeObject = colladaData.DaeObject;
    }

    [TestMethod]
    public void BehrRifle_312IvoChrFile()
    {
        var args = new string[] { $@"{userHome}\OneDrive\ResourceFiles\SC\3.12.0\brfl_fps_behr_p4ar.chr", "-dds", "-dae" };
        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);
        CryEngine cryData = new(args[0], testUtils.argsHandler.PackFileSystem);
        cryData.ProcessCryengineFiles();

        var colladaData = new ColladaModelRenderer(testUtils.argsHandler, cryData);
        colladaData.GenerateDaeObject();
        var daeObject = colladaData.DaeObject;

        //Assert.AreEqual(17, cryData.Materials.Count);

        testUtils.ValidateColladaXml(colladaData);
    }

    [TestMethod]
    public void BehrRifle_312IvoSkinFile()
    {
        var args = new string[] { $@"{userHome}\OneDrive\ResourceFiles\SC\3.12.0\brfl_fps_behr_p4ar_parts.skin", "-dds", "-dae" };
        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);
        CryEngine cryData = new(args[0], testUtils.argsHandler.PackFileSystem);
        cryData.ProcessCryengineFiles();

        var colladaData = new ColladaModelRenderer(testUtils.argsHandler, cryData);
        colladaData.GenerateDaeObject();
        var daeObject = colladaData.DaeObject;

        testUtils.ValidateColladaXml(colladaData);
    }

    [TestMethod]
    public void BehrRifleParts_34_ChCrSkinFile()
    {
        var args = new string[] { $@"{userHome}\OneDrive\ResourceFiles\SC\brfl_fps_behr_p4ar_parts_3.4.skin", "-dds", "-dae" };
        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);
        CryEngine cryData = new(args[0], testUtils.argsHandler.PackFileSystem);
        cryData.ProcessCryengineFiles();

        var colladaData = new ColladaModelRenderer(testUtils.argsHandler, cryData);
        colladaData.GenerateDaeObject();
        var daeObject = colladaData.DaeObject;

        testUtils.ValidateColladaXml(colladaData);
    }

    [TestMethod]
    public void BehrRifleParts_34_Gltf_ChCrSkinFile()
    {
        var args = new string[] { $@"{userHome}\OneDrive\ResourceFiles\SC\brfl_fps_behr_p4ar_parts_3.4.skin", "-dds", "-dae" };
        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);
        CryEngine cryData = new(args[0], testUtils.argsHandler.PackFileSystem);
        cryData.ProcessCryengineFiles();

        GltfModelRenderer gltfRenderer = new(testUtils.argsHandler, cryData, true, false);
        var gltfData = gltfRenderer.GenerateGltfObject();
        gltfRenderer.Render();
    }

    [TestMethod]
    public void AEGS_Avenger()
    {
        var args = new string[] { $@"{userHome}\OneDrive\ResourceFiles\SC\AEGS_Avenger.cga", "-dds", "-dae" };
        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);
        var cryData = new CryEngine(args[0], testUtils.argsHandler.PackFileSystem);
        cryData.ProcessCryengineFiles();

        var colladaData = new ColladaModelRenderer(testUtils.argsHandler, cryData);
        var daeObject = colladaData.DaeObject;
        colladaData.GenerateDaeObject();
        // Make sure Rotations are still right
        var noseNode = daeObject.Library_Visual_Scene.Visual_Scene[0].Node[0].node[0];
        Assert.AreEqual("Nose", noseNode.ID);
        Assert.AreEqual("Front_LG_Door_Left", noseNode.node[28].ID);
        Assert.AreEqual("-0.300001 0.512432 -1.835138", noseNode.node[28].Translate[0].Value_As_String);
        Assert.AreEqual("-1 -0 -0 200.259949", noseNode.node[28].Rotate[0].Value_As_String);

        Assert.AreEqual(29, colladaData.DaeObject.Library_Materials.Material.Length);
        Assert.AreEqual(88, colladaData.DaeObject.Library_Images.Image.Length);
        testUtils.ValidateColladaXml(colladaData);
    }

    [TestMethod]
    public void AEGS_Avenger_Gltf()
    {
        var args = new string[] { @"d:\depot\sc2\data\objects\spaceships\ships\aegs\Avenger\AEGS_Avenger.cga", "-dds", "-objectdir", @"d:\depot\sc2\data" };
        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);
        CryEngine cryData = new(args[0], testUtils.argsHandler.PackFileSystem);
        cryData.ProcessCryengineFiles();

        GltfModelRenderer gltfRenderer = new(testUtils.argsHandler, cryData, true, false);
        var gltfData = gltfRenderer.GenerateGltfObject();

        Assert.AreEqual(22, gltfData.Materials.Count);
        Assert.AreEqual(34, gltfData.Meshes.Count);

        // Nodes check
        Assert.AreEqual(116, gltfData.Nodes.Count);
        Assert.AreEqual("AEGS_Avenger", gltfData.Nodes[0].Name);
        Assert.AreEqual("Nose", gltfData.Nodes[1].Name);
        Assert.AreEqual("UI_Helper", gltfData.Nodes[2].Name);

        AssertExtensions.AreEqual(new System.Collections.Generic.List<float> { 0, 0, 0, 1 }, gltfData.Nodes[0].Rotation, TestUtils.delta);
        AssertExtensions.AreEqual(new System.Collections.Generic.List<float> { -0.0f, -0.0f, 0.0f, 1f }, gltfData.Nodes[1].Rotation, TestUtils.delta);
        AssertExtensions.AreEqual(new System.Collections.Generic.List<float> { 0, 0, 0, 1 }, gltfData.Nodes[2].Rotation, TestUtils.delta);

        AssertExtensions.AreEqual(new System.Collections.Generic.List<float> { 0, 0, 0 }, gltfData.Nodes[0].Translation, TestUtils.delta);
        AssertExtensions.AreEqual(new System.Collections.Generic.List<float> { 0.0f, -0.473000f, -5.702999f }, gltfData.Nodes[1].Translation, TestUtils.delta);
        AssertExtensions.AreEqual(new System.Collections.Generic.List<float> { 0f, 0.795895f, -1.898374f }, gltfData.Nodes[2].Translation, TestUtils.delta);

        // Grip.  Test loc and rotation on a node with a parent
        var grip = gltfData.Nodes.Where(x => x.Name == "Grip").FirstOrDefault();
        AssertExtensions.AreEqual(new System.Collections.Generic.List<float> { -1.41231394f, 0.0213999934f, -1.660965f }, grip.Translation, TestUtils.delta);
        AssertExtensions.AreEqual(new System.Collections.Generic.List<float> { 0.464955121f, -0.221349508f, 0.769474566f, 0.3777963f }, grip.Rotation, TestUtils.delta);

        Assert.AreEqual(3, gltfData.Nodes[0].Children.Count); // Root
        Assert.AreEqual(44, gltfData.Nodes[1].Children.Count);
        Assert.AreEqual(0, gltfData.Nodes[2].Children.Count);

        // Accessors check
        Assert.AreEqual(282, gltfData.Accessors.Count);
    }

    [TestMethod]
    public void AEGS_GladiusLandingGearFront_CHR()
    {
        var args = new string[] { $@"{userHome}\OneDrive\ResourceFiles\SC\ivo\AEGS_Gladius_LandingGear_Front_CHR.chr", "-dds", "-dae", "-objectdir", @"d:\depot\sc2\data" };
        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);
        CryEngine cryData = new(args[0], testUtils.argsHandler.PackFileSystem);
        cryData.ProcessCryengineFiles();

        var colladaData = new ColladaModelRenderer(testUtils.argsHandler, cryData);
        var daeObject = colladaData.DaeObject;
        colladaData.GenerateDaeObject();

        Assert.IsFalse(cryData.Models[0].HasGeometry);
    }

    [TestMethod]
    public void SC_hangar_asteroid_controlroom_fan()
    {
        var args = new string[] { $@"{userHome}\OneDrive\ResourceFiles\hangar_asteroid_controlroom_fan.cgf", "-dds", "-dae", "-objectdir", @"..\..\ResourceFiles\" };
        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);
        CryEngine cryData = new CryEngine(args[0], testUtils.argsHandler.PackFileSystem);
        cryData.ProcessCryengineFiles();

        ColladaModelRenderer colladaData = new ColladaModelRenderer(testUtils.argsHandler, cryData);
        colladaData.GenerateDaeObject();

        var geometries = colladaData.DaeObject.Library_Geometries.Geometry;
        Assert.AreEqual(3, geometries.Length);

        testUtils.ValidateColladaXml(colladaData);
    }

    [TestMethod]
    public void SC_BehrRifle_34()
    {
        var args = new string[] {
            $@"{userHome}\OneDrive\ResourceFiles\SC\brfl_fps_behr_p4ar_parts_3.4.skin",
            "-dds", "-dae", "-objectdir", @"..\..\ResourceFiles\" };
        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);
        CryEngine cryData = new CryEngine(args[0], testUtils.argsHandler.PackFileSystem);
        cryData.ProcessCryengineFiles();

        ColladaModelRenderer colladaData = new ColladaModelRenderer(testUtils.argsHandler, cryData);
        colladaData.GenerateDaeObject();

        var controllers = colladaData.DaeObject.Library_Controllers.Controller;
        var geometries = colladaData.DaeObject.Library_Geometries.Geometry;
        Assert.AreEqual(1, controllers.Length);
        Assert.AreEqual(1, geometries.Length);

        var mesh = geometries[0].Mesh;
        Assert.AreEqual(4, mesh.Source.Length);
        Assert.AreEqual("brfl_fps_behr_p4ar_parts-vertices", mesh.Vertices.ID);
        Assert.AreEqual(9, mesh.Triangles.Length);
        Assert.AreEqual(78, mesh.Triangles[0].Count);
        Assert.AreEqual(134, mesh.Triangles[8].Count);

        testUtils.ValidateColladaXml(colladaData);
    }

    [TestMethod]
    public void BehrRifle_312_NonIvo()
    {
        var args = new string[] {
            $@"{userHome}\OneDrive\ResourceFiles\SC\3.12.0\brfl_fps_behr_p4ar_body.cgf",
            "-dds", "-dae" };
        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);
        CryEngine cryData = new CryEngine(args[0], testUtils.argsHandler.PackFileSystem);
        cryData.ProcessCryengineFiles();

        ColladaModelRenderer colladaData = new ColladaModelRenderer(testUtils.argsHandler, cryData);
        colladaData.GenerateDaeObject();

        // Geometry Library checks
        var geometries = colladaData.DaeObject.Library_Geometries.Geometry;
        Assert.AreEqual(1, geometries.Length);

        var mesh = geometries[0].Mesh;
        Assert.AreEqual(4, mesh.Source.Length);
        Assert.AreEqual("brfl_fps_behr_p4ar_body-vertices", mesh.Vertices.ID);
        Assert.AreEqual(13, mesh.Triangles.Length);
        Assert.AreEqual(84, mesh.Triangles[0].Count);
        Assert.AreEqual(1460, mesh.Triangles[8].Count);

        var vertices = mesh.Source[0];
        var normals = mesh.Source[1];
        var uvs = mesh.Source[2];
        var colors = mesh.Source[3];
        Assert.AreEqual("brfl_fps_behr_p4ar_body-mesh-pos", vertices.ID);
        Assert.AreEqual("brfl_fps_behr_p4ar_body-pos", vertices.Name);
        Assert.AreEqual("brfl_fps_behr_p4ar_body-mesh-norm", normals.ID);
        Assert.AreEqual("brfl_fps_behr_p4ar_body-norm", normals.Name);
        Assert.AreEqual("brfl_fps_behr_p4ar_body-mesh-UV", uvs.ID);
        Assert.AreEqual("brfl_fps_behr_p4ar_body-UV", uvs.Name);
        Assert.AreEqual("brfl_fps_behr_p4ar_body-mesh-color", colors.ID);
        Assert.AreEqual("brfl_fps_behr_p4ar_body-color", colors.Name);
        Assert.AreEqual(56058, vertices.Float_Array.Count);
        Assert.AreEqual("brfl_fps_behr_p4ar_body-mesh-pos-array", vertices.Float_Array.ID);
        Assert.IsTrue(vertices.Float_Array.Value_As_String.StartsWith("-0.020622 0.180945 0.097055 -0.020622 0.178238 0.092718 -0.020622 0.175470 0.097055 -0.020622 0.175408 0.105175 -0.020622"));
        Assert.AreEqual((uint)18686, vertices.Technique_Common.Accessor.Count);
        Assert.AreEqual((uint)3, vertices.Technique_Common.Accessor.Stride);
        Assert.AreEqual(56058, normals.Float_Array.Count);
        Assert.AreEqual((uint)18686, normals.Technique_Common.Accessor.Count);
        Assert.AreEqual((uint)3, normals.Technique_Common.Accessor.Stride);
        Assert.AreEqual(37372, uvs.Float_Array.Count);
        Assert.AreEqual((uint)18686, uvs.Technique_Common.Accessor.Count);
        Assert.AreEqual((uint)2, uvs.Technique_Common.Accessor.Stride);
        Assert.AreEqual(74744, colors.Float_Array.Count);

        testUtils.ValidateColladaXml(colladaData);
    }

    [TestMethod]
    public void DRAK_Buccaneer_Landing_Gear_Front_Skin()
    {
        var args = new string[] {
            $@"{userHome}\OneDrive\ResourceFiles\SC\ivo\DRAK_Buccaneer_Landing_Gear_Front_Skin.skin",
            "-dds", "-dae" };
        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);
        CryEngine cryData = new(args[0], testUtils.argsHandler.PackFileSystem);
        cryData.ProcessCryengineFiles();

        ColladaModelRenderer colladaData = new(testUtils.argsHandler, cryData);
        colladaData.GenerateDaeObject();

        // Geometry Library checks
        var geometries = colladaData.DaeObject.Library_Geometries.Geometry;
        Assert.AreEqual(1, geometries.Length);

        // Materials check
        var materials = colladaData.DaeObject.Library_Materials.Material;
        Assert.AreEqual(25, materials.Length);
    }

    [TestMethod]
    public void Mobiglass()
    {
        var args = new string[] {
            $@"{userHome}\OneDrive\ResourceFiles\SC\ivo\f_mobiglas_civilian_01.skin",
            "-dds", "-dae" };
        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);
        CryEngine cryData = new(args[0], testUtils.argsHandler.PackFileSystem);
        cryData.ProcessCryengineFiles();

        ColladaModelRenderer colladaData = new(testUtils.argsHandler, cryData);
        colladaData.GenerateDaeObject();

        // Geometry Library checks
        var geometries = colladaData.DaeObject.Library_Geometries.Geometry;
        Assert.AreEqual(1, geometries.Length);

        // Materials check
        var materials = colladaData.DaeObject.Library_Materials.Material;
        Assert.AreEqual(5, materials.Length);
    }

    [TestMethod]
    public void Mobiglass_Gltf()
    {
        var args = new string[] {
            $@"{userHome}\OneDrive\ResourceFiles\SC\ivo\f_mobiglas_civilian_01.skin",
            "-dds", "-dae" };
        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);
        CryEngine cryData = new(args[0], testUtils.argsHandler.PackFileSystem);
        cryData.ProcessCryengineFiles();

        GltfModelRenderer gltfRenderer = new(testUtils.argsHandler, cryData, true, false);
        var gltfData = gltfRenderer.GenerateGltfObject();
        gltfRenderer.Render();
    }

    [TestMethod]
    public void Avenger_Ramp_Exterior()
    {
        var args = new string[] { $@"D:\depot\SC2\Data\Objects\Spaceships\Ships\AEGS\Avenger\aegs_avenger_ramp_exterior.cga", "-dds", "-gltf" };
        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);
        CryEngine cryData = new(args[0], testUtils.argsHandler.PackFileSystem);
        cryData.ProcessCryengineFiles();

        GltfModelRenderer gltfRenderer = new(testUtils.argsHandler, cryData, true, false);
        var gltfData = gltfRenderer.GenerateGltfObject();
        var geometries = gltfData.Meshes;

        gltfRenderer.Render();
    }


    [TestMethod]
    public void Glaive()
    {
        var args = new string[] {
            $@"{userHome}\OneDrive\ResourceFiles\SC\3.12.0\VNCL_Glaive.cga",
            "-dds", "-dae", "-objectdir", @"..\..\ResourceFiles\" };

        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);
        CryEngine cryData = new(args[0], testUtils.argsHandler.PackFileSystem);
        cryData.ProcessCryengineFiles();

        ColladaModelRenderer colladaData = new(testUtils.argsHandler, cryData);
        colladaData.GenerateDaeObject();

        // Geometry Library checks
        var geometries = colladaData.DaeObject.Library_Geometries.Geometry;
        Assert.AreEqual(96, geometries.Length);

        // Materials check
        var materials = colladaData.DaeObject.Library_Materials.Material;
        Assert.AreEqual(20, materials.Length);
    }
}
