using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;
using System.Text;

public class ObjExporter : ScriptableObject
{
	[MenuItem("File/Export/Wavefront OBJ")]
	static void DoExportWSubmeshes() {
		DoExport(true);
	}

	[MenuItem("File/Export/Wavefront OBJ (No Submeshes)")]
	static void DoExportWOSubmeshes() {
		DoExport(false);
	}


	static void DoExport(bool makeSubmeshes) {
		if (Selection.gameObjects.Length == 0) {
			Debug.Log("Didn't Export Any Meshes; Nothing was selected!");
			return;
		}

		string meshName = Selection.gameObjects[0].name;
		string fileName = EditorUtility.SaveFilePanel("Export .obj file", "", meshName, "obj");

		ObjExporterScript.Start();

		StringBuilder meshString = new StringBuilder();

		meshString.Append("#" + meshName + ".obj"
							+ "\n#" + System.DateTime.Now.ToLongDateString()
							+ "\n#" + System.DateTime.Now.ToLongTimeString()
							+ "\n#-------"
							+ "\n\n");

		Transform t = Selection.gameObjects[0].transform;

		Vector3 originalPosition = t.position;
		t.position = Vector3.zero;

		if (!makeSubmeshes) {
			meshString.Append("g ").Append(t.name).Append("\n");
		}
		meshString.Append(processTransform(t, makeSubmeshes));

		WriteToFile(meshString.ToString(), fileName);

		t.position = originalPosition;

		ObjExporterScript.End();
		Debug.Log("Exported Mesh: " + fileName);
	}

	static string processTransform(Transform t, bool makeSubmeshes) {
		StringBuilder meshString = new StringBuilder();

		meshString.Append("#" + t.name
						+ "\n#-------"
						+ "\n");

		if (makeSubmeshes) {
			meshString.Append("g ").Append(t.name).Append("\n");
		}

		MeshFilter mf = t.GetComponent<MeshFilter>();
		if (mf) {
			meshString.Append(ObjExporterScript.MeshToString(mf, t));
		}

		for (int i = 0; i < t.childCount; i++) {
			meshString.Append(processTransform(t.GetChild(i), makeSubmeshes));
		}

		return meshString.ToString();
	}

	static void WriteToFile(string s, string filename) {
		using (StreamWriter sw = new StreamWriter(filename)) {
			sw.Write(s);
		}
	}
}