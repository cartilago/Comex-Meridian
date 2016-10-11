/*     INFINITY CODE 2013-2016      */
/*   http://www.infinity-code.com   */

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// Built-in buildings generator.
/// </summary>
[AddComponentMenu("")]
public class OnlineMapsBuildingBuiltIn : OnlineMapsBuildingBase
{
    //private OnlineMapsOSMWay way;
    public static List<int> roofIndices;
    public static List<OnlineMapsOSMNode> usedNodes;

    private static void AnalizeHouseRoofType(OnlineMapsOSMWay way, ref float baseHeight, ref OnlineMapsBuildingRoofType roofType, ref float roofHeight)
    {
        string roofShape = way.GetTagValue("roof:shape");
        string roofHeightStr = way.GetTagValue("roof:height");
        string minHeightStr = way.GetTagValue("min_height");
        if (!String.IsNullOrEmpty(roofShape))
        {
            if ((roofShape == "dome" || roofShape == "pyramidal") && !String.IsNullOrEmpty(roofHeightStr))
            {
                GetHeightFromString(roofHeightStr, ref roofHeight);
                baseHeight -= roofHeight;
                roofType = OnlineMapsBuildingRoofType.dome;
            }
        }
        else if (!String.IsNullOrEmpty(roofHeightStr))
        {
            GetHeightFromString(roofHeightStr, ref roofHeight);
            baseHeight -= roofHeight;
            roofType = OnlineMapsBuildingRoofType.dome;
        }
        else if (!String.IsNullOrEmpty(minHeightStr))
        {
            float totalHeight = baseHeight;
            GetHeightFromString(minHeightStr, ref baseHeight);
            roofHeight = totalHeight - baseHeight;
            roofType = OnlineMapsBuildingRoofType.dome;
        }
    }

    private static void AnalizeHouseTags(OnlineMapsOSMWay way, ref Material wallMaterial, ref Material roofMaterial, ref float baseHeight)
    {
        string heightStr = way.GetTagValue("height");
        bool hasHeight = false;

        if (!string.IsNullOrEmpty(heightStr)) hasHeight = GetHeightFromString(heightStr, ref baseHeight);

        if (!hasHeight)
        {
            string levelsStr = way.GetTagValue("building:levels");
            if (!String.IsNullOrEmpty(levelsStr))
            {
                float countLevels = 0;
                if (float.TryParse(levelsStr, out countLevels))
                {
                    baseHeight = countLevels * OnlineMapsBuildings.instance.levelHeight;
                    hasHeight = true;
                }
            }
        }

        if (!hasHeight)
        {
            if (OnlineMapsBuildings.instance.OnGenerateBuildingHeight != null) baseHeight = OnlineMapsBuildings.instance.OnGenerateBuildingHeight(way);
            else baseHeight = Random.Range(OnlineMapsBuildings.instance.levelsRange.min, OnlineMapsBuildings.instance.levelsRange.max) * OnlineMapsBuildings.instance.levelHeight;
        }

        if (baseHeight < OnlineMapsBuildings.instance.minHeight) baseHeight = OnlineMapsBuildings.instance.minHeight;

        string colorStr = way.GetTagValue("building:colour");
        if (!String.IsNullOrEmpty(colorStr)) wallMaterial.color = roofMaterial.color = StringToColor(colorStr);
    }

    /// <summary>
    /// Creates a new building, based on Open Street Map.
    /// </summary>
    /// <param name="container">Reference to OnlineMapsBuildings.</param>
    /// <param name="way">Way of building.</param>
    /// <param name="nodes">Nodes obtained from Open Street Maps.</param>
    /// <returns>Building instance.</returns>
    public static OnlineMapsBuildingBase Create(OnlineMapsBuildings container, OnlineMapsOSMWay way, Dictionary<string, OnlineMapsOSMNode> nodes)
    {
        if (CheckIgnoredBuildings(way)) return null;

        if (usedNodes == null) usedNodes = new List<OnlineMapsOSMNode>(30);
        way.GetNodes(nodes, usedNodes);
        List<Vector3> points = GetLocalPoints(usedNodes);

        if (points.Count < 3) return null;
        if (points[0] == points[points.Count - 1]) points.RemoveAt(points.Count - 1);
        if (points.Count < 3) return null;

        for (int i = 0; i < points.Count; i++)
        {
            int prev = i - 1;
            if (prev < 0) prev = points.Count - 1;

            int next = i + 1;
            if (next >= points.Count) next = 0;

            float a1 = OnlineMapsUtils.Angle2D(points[prev], points[i]);
            float a2 = OnlineMapsUtils.Angle2D(points[i], points[next]);

            if (Mathf.Abs(a1 - a2) < 5)
            {
                points.RemoveAt(i);
                i--;
            }
        }

        if (points.Count < 3) return null;

        Vector4 cp = new Vector4(float.MaxValue, float.MaxValue, float.MinValue, float.MinValue);
        foreach (Vector3 point in points)
        {
            if (point.x < cp.x) cp.x = point.x;
            if (point.z < cp.y) cp.y = point.z;
            if (point.x > cp.z) cp.z = point.x;
            if (point.z > cp.w) cp.w = point.z;
        }
        
        Vector3 centerPoint = new Vector3((cp.z + cp.x) / 2, 0, (cp.y + cp.w) / 2);

        for (int i = 0; i < points.Count; i++) points[i] -= centerPoint;

        bool generateWall = true;

        if (way.HasTagKey("building"))
        {
            string buildingType = way.GetTagValue("building");
            if (buildingType == "roof") generateWall = false;
        }

        float baseHeight = 15;
        float roofHeight = 0;

        OnlineMapsBuildingMaterial material = GetRandomMaterial(container);
        Material wallMaterial;
        Material roofMaterial;
        Vector2 scale = Vector2.one;

        if (material != null)
        {
            wallMaterial = new Material(material.wall);
            roofMaterial = new Material(material.roof);
            scale = material.scale;
        }
        else
        {
            Shader shader = Shader.Find("Diffuse");
            wallMaterial = new Material(shader);
            roofMaterial = new Material(shader);
        }

        OnlineMapsBuildingRoofType roofType = OnlineMapsBuildingRoofType.flat;
        AnalizeHouseTags(way, ref wallMaterial, ref roofMaterial, ref baseHeight);
        AnalizeHouseRoofType(way, ref baseHeight, ref roofType, ref roofHeight);

        GameObject houseGO = CreateGameObject(way.id);
        MeshRenderer renderer = houseGO.AddComponent<MeshRenderer>();
        MeshFilter meshFilter = houseGO.AddComponent<MeshFilter>();

        Mesh mesh = new Mesh {name = way.id};

        meshFilter.sharedMesh = mesh;
        renderer.sharedMaterials = new []
        {
            wallMaterial,
            roofMaterial
        };

        OnlineMapsBuildingBuiltIn building = houseGO.AddComponent<OnlineMapsBuildingBuiltIn>();
        houseGO.transform.localPosition = centerPoint;
        houseGO.transform.localRotation = Quaternion.Euler(Vector3.zero);

        Vector2 centerCoords = Vector2.zero;
        float minCX = float.MaxValue, minCY = float.MaxValue, maxCX = float.MinValue, maxCY = float.MinValue;

        foreach (OnlineMapsOSMNode node in usedNodes)
        {
            Vector2 nodeCoords = node;
            centerCoords += nodeCoords;
            if (nodeCoords.x < minCX) minCX = nodeCoords.x;
            if (nodeCoords.y < minCY) minCY = nodeCoords.y;
            if (nodeCoords.x > maxCX) maxCX = nodeCoords.x;
            if (nodeCoords.y > maxCY) maxCY = nodeCoords.y;
        }

        building.id = way.id;
        building.initialZoom = OnlineMaps.instance.zoom;
        building.centerCoordinates = new Vector2((maxCX + minCX) / 2, (maxCY + minCY) / 2);
        building.boundsCoords = new Bounds(building.centerCoordinates, new Vector3(maxCX - minCX, maxCY - minCY));

        int wallVerticesCount = (points.Count + 1) * 2;
        int roofVerticesCount = points.Count;
        int verticesCount = wallVerticesCount + roofVerticesCount;
        int countTriangles = wallVerticesCount * 3;

        List<Vector3> vertices = new List<Vector3>(verticesCount);
        List<Vector2> uvs = new List<Vector2>(verticesCount);
        List<int> wallTriangles = new List<int>(countTriangles);
        List<int> roofTriangles = new List<int>();

        if (generateWall) building.CreateHouseWall(points, baseHeight, wallMaterial, scale, ref vertices, ref uvs, ref wallTriangles);
        building.CreateHouseRoof(points, baseHeight, roofHeight, roofType, ref vertices, ref uvs, ref roofTriangles);

        if (building.hasErrors)
        {
            OnlineMapsUtils.DestroyImmediate(building.gameObject);
            return null;
        }

        mesh.vertices = vertices.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.subMeshCount = 2;
        mesh.SetTriangles(wallTriangles.ToArray(), 0);
        mesh.SetTriangles(roofTriangles.ToArray(), 1);

        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

        building.buildingCollider = houseGO.AddComponent<MeshCollider>();
        (building.buildingCollider as MeshCollider).sharedMesh = mesh;

        return building;
    }

    private void CreateHouseRoof(List<Vector3> baseVerticles, float baseHeight, float roofHeight, OnlineMapsBuildingRoofType roofType, ref List<Vector3> vertices, ref List<Vector2> uvs, ref List<int> triangles)
    {
        float[] roofPoints = new float[baseVerticles.Count * 2];
        List<Vector3> roofVertices = new List<Vector3>(baseVerticles.Count);

        try
        {
            int countVertices = CreateHouseRoofVerticles(baseVerticles, roofVertices, roofPoints, baseHeight);
            CreateHouseRoofTriangles(countVertices, roofVertices, roofType, roofPoints, baseHeight, roofHeight, ref triangles);

            if (triangles.Count == 0)
            {
                hasErrors = true;
                return;
            }

            Vector3 side1 = roofVertices[triangles[1]] - roofVertices[triangles[0]];
            Vector3 side2 = roofVertices[triangles[2]] - roofVertices[triangles[0]];
            Vector3 perp = Vector3.Cross(side1, side2);

            bool reversed = perp.y < 0;
            if (reversed) triangles.Reverse();

            float minX = float.MaxValue;
            float minZ = float.MaxValue;
            float maxX = float.MinValue;
            float maxZ = float.MinValue;

            foreach (Vector3 v in roofVertices)
            {
                if (v.x < minX) minX = v.x;
                if (v.z < minZ) minZ = v.z;
                if (v.x > maxX) maxX = v.x;
                if (v.z > maxZ) maxZ = v.z;
            }

            float offX = maxX - minX;
            float offZ = maxZ - minZ;

            foreach (Vector3 v in roofVertices) uvs.Add(new Vector2((v.x - minX) / offX, (v.z - minZ) / offZ));
            
            //uvs.AddRange(roofVertices.Select(v => new Vector2((v.x - minX) / offX, (v.z - minZ) / offZ)));

            int triangleOffset = vertices.Count;
            for (int i = 0; i < triangles.Count; i++) triangles[i] += triangleOffset;

            vertices.AddRange(roofVertices);
        }
        catch (Exception)
        {
            Debug.Log(triangles.Count + "   " + roofVertices.Count);
            hasErrors = true;
            throw;
        }
    }

    private static void CreateHouseRoofDome(float height, List<Vector3> vertices, List<int> triangles)
    {
        Vector3 roofTopPoint = Vector3.zero;
        roofTopPoint = vertices.Aggregate(roofTopPoint, (current, point) => current + point) / vertices.Count;
        roofTopPoint.y = height;
        int vIndex = vertices.Count;

        for (int i = 0; i < vertices.Count; i++)
        {
            int p1 = i;
            int p2 = i + 1;
            if (p2 >= vertices.Count) p2 -= vertices.Count;

            triangles.AddRange(new[] { p1, p2, vIndex });
        }

        vertices.Add(roofTopPoint);
    }

    private static void CreateHouseRoofTriangles(int countVertices, List<Vector3> vertices, OnlineMapsBuildingRoofType roofType, float[] roofPoints, float baseHeight, float roofHeight, ref List<int> triangles)
    {
        if (roofType == OnlineMapsBuildingRoofType.flat)
        {
            if (roofIndices == null) roofIndices = new List<int>(60);
            triangles.AddRange(OnlineMapsUtils.Triangulate(roofPoints, countVertices, roofIndices));
        }
        else if (roofType == OnlineMapsBuildingRoofType.dome) CreateHouseRoofDome(baseHeight + roofHeight, vertices, triangles);
    }

    private static int CreateHouseRoofVerticles(List<Vector3> baseVerticles, List<Vector3> verticles, float[] roofPoints, float baseHeight)
    {
        float topPoint = baseHeight * OnlineMapsBuildings.instance.heightScale;
        int countVertices = 0;

        for (int i = 0; i < baseVerticles.Count; i++)
        {
            Vector3 p = baseVerticles[i];
            float px = p.x;
            float pz = p.z;

            bool hasVerticle = false;

            for (int j = 0; j < countVertices * 2; j += 2)
            {
                if (roofPoints[j] == px && roofPoints[j + 1] == pz)
                {
                    hasVerticle = true;
                    break;
                }
            }

            if (!hasVerticle)
            {
                int cv2 = countVertices * 2;

                roofPoints[cv2] = px;
                roofPoints[cv2 + 1] = pz;
                verticles.Add(new Vector3(px, topPoint, pz));

                countVertices++;
            }
        }

        return countVertices;
    }

    private void CreateHouseWall(List<Vector3> baseVerticles, float baseHeight, Material material, Vector2 materialScale, ref List<Vector3> vertices, ref List<Vector2> uvs, ref List<int> triangles)
    {
        CreateHouseWallMesh(baseVerticles, baseHeight, false, ref vertices, ref uvs, ref triangles);

        Vector2 scale = material.mainTextureScale;
        scale.x *= perimeter / 100 * materialScale.x;
        scale.y *= baseHeight / 30 * materialScale.y;
        material.mainTextureScale = scale;
    }

    private void CreateHouseWallMesh(List<Vector3> baseVerticles, float baseHeight, bool inverted, ref List<Vector3> vertices, ref List<Vector2> uvs, ref List<int> triangles)
    {
        bool reversed = CreateHouseWallVerticles(baseHeight, baseVerticles, vertices, uvs);
        if (inverted) reversed = !reversed;
        CreateHouseWallTriangles(vertices, reversed, ref triangles);
    }

    private static void CreateHouseWallTriangles(List<Vector3> vertices, bool reversed, ref List<int> triangles)
    {
        int countVertices = vertices.Count;
        for (int i = 0; i < countVertices / 2 - 1; i++)
        {
            int p1 = i * 2;
            int p2 = p1 + 2;
            int p3 = p2 + 1;
            int p4 = p1 + 1;

            if (p2 >= countVertices) p2 -= countVertices;
            if (p3 >= countVertices) p3 -= countVertices;

            if (reversed)
            {
                triangles.Add(p1);
                triangles.Add(p4);
                triangles.Add(p3);
                triangles.Add(p1);
                triangles.Add(p3);
                triangles.Add(p2);
            }
            else
            {
                triangles.Add(p2);
                triangles.Add(p3);
                triangles.Add(p1);
                triangles.Add(p3);
                triangles.Add(p4);
                triangles.Add(p1);
            }
        }
    }

    private bool CreateHouseWallVerticles(float baseHeight, List<Vector3> baseVerticles, List<Vector3> vertices, List<Vector2> uvs)
    {
        float topPoint = baseHeight * OnlineMapsBuildings.instance.heightScale;

        int baseVerticesCount = baseVerticles.Count;
        for (int i = 0; i <= baseVerticesCount; i++)
        {
            int j = i;
            if (j >= baseVerticesCount) j -= baseVerticesCount;

            Vector3 p = baseVerticles[j];
            Vector3 tv = new Vector3(p.x, topPoint, p.z);

            vertices.Add(p);
            vertices.Add(tv);
        }

        float currentDistance = 0;
        int countVertices = vertices.Count;
        int halfVerticesCount = countVertices / 2;
        perimeter = 0;

        for (int i = 0; i <= halfVerticesCount; i++)
        {
            int i1 = i * 2;
            int i2 = i * 2 + 2;
            
            while (i1 >= countVertices) i1 -= countVertices;
            while (i2 >= countVertices) i2 -= countVertices;

            float magnitude = (vertices[i1] - vertices[i2]).magnitude;
            perimeter += magnitude;

            if (i < halfVerticesCount)
            {
                float curU = currentDistance / perimeter;
                uvs.Add(new Vector2(curU, 0));
                uvs.Add(new Vector2(curU, 1));

                currentDistance += magnitude;
            }
        }

        int southIndex = -1;
        float southZ = float.MaxValue;

        for (int i = 0; i < baseVerticesCount; i++)
        {
            if (baseVerticles[i].z < southZ)
            {
                southZ = baseVerticles[i].z;
                southIndex = i;
            }
        }

        int prevIndex = southIndex - 1;
        if (prevIndex < 0) prevIndex = baseVerticesCount - 1;

        int nextIndex = southIndex + 1;
        if (nextIndex >= baseVerticesCount) nextIndex = 0;

        float angle1 = OnlineMapsUtils.Angle2D(baseVerticles[southIndex], baseVerticles[nextIndex]);
        float angle2 = OnlineMapsUtils.Angle2D(baseVerticles[southIndex], baseVerticles[prevIndex]);

        return angle1 < angle2;
    }

    private static bool GetHeightFromString(string str, ref float height)
    {
        if (!String.IsNullOrEmpty(str))
        {
            if (!float.TryParse(str, out height))
            {
                if (str.Substring(str.Length - 2, 2) == "cm")
                {
                    if (float.TryParse(str.Substring(0, str.Length - 2), out height))
                    {
                        height /= 10;
                        return true;
                    }
                }
                else if (str.Substring(str.Length - 1, 1) == "m")
                {
                    return float.TryParse(str.Substring(0, str.Length - 1), out height);
                }
            }
        }
        return false;
    }

    private static OnlineMapsBuildingMaterial GetRandomMaterial(OnlineMapsBuildings container)
    {
        if (container.materials == null || container.materials.Length == 0) return null;

        return container.materials[Random.Range(0, container.materials.Length)];
    }

    private static Color StringToColor(string str)
    {
        str = str.ToLower();
        if (str == "black") return Color.black;
        if (str == "blue") return Color.blue;
        if (str == "cyan") return Color.cyan;
        if (str == "gray") return Color.gray;
        if (str == "green") return Color.green;
        if (str == "magenta") return Color.magenta;
        if (str == "red") return Color.red;
        if (str == "white") return Color.white;
        if (str == "yellow") return Color.yellow;

        try
        {
            string hex = (str + "000000").Substring(1, 6);
            byte[] cb =
                Enumerable.Range(0, hex.Length)
                    .Where(x => x % 2 == 0)
                    .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                    .ToArray();
            return new Color32(cb[0], cb[1], cb[2], 255);
        }
        catch
        {
            return Color.white;
        }
    }
}