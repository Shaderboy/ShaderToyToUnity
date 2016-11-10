using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using NUnit.Framework;
using System.IO;
using System.Text.RegularExpressions;

internal static class ShadertoyToUnity{

	private const string MenuRoot = "ShaderToy Converter/"; 

	[MenuItem (MenuRoot + "ShaderToy->Unity")]
	static void StToUnity() {
		string path = AssetDatabase.GetAssetPath (Selection.activeObject);
		//string stCode = File.ReadAllText(AssetDatabase.GetAssetPath(Selection.activeObject.GetInstanceID()));
		string stCode = File.ReadAllText(path);
		string code = File.ReadAllText("Assets/ShaderToyConverter/Base.shader");

		code = code.Replace("Shader \"Paintings/Base\"{", "Shader \"ShaderToy/" + Path.GetFileNameWithoutExtension(path) + "\"{");
		code = code.Replace("fixed4 frag (v2f i) : SV_Target{", "");
		code = code.Replace("return fixed4(0., 0., 0., 0.);}", "");
		code = code.Replace("//Shadertoy Code Goes Here", stCode);
		code = code.Replace("void mainImage( out vec4 fragColor, in vec2 fragCoord )\n{", "fixed4 frag (v2f i) : SV_Target{");
		code = code.Replace("void mainImage( out vec4 fragColor, in vec2 fragCoord ){", "fixed4 frag (v2f i) : SV_Target{");
		code = code.Replace("fragCoord.xy", "i.screenCoord.xy * _ScreenParams.xy");
		code = code.Replace("iResolution", "_ScreenParams");
		code = code.Replace("vec", "float");
		code = code.Replace("mix", "lerp");
		code = code.Replace("fract", "frac");
		code = code.Replace("mod", "fmod");
		code = code.Replace("iGlobalTime", "_Time.y");
		code = code.Replace("fragColor =", "return");
		code = code.Replace("Texture2D", "Tex2D");
		code = code.Replace("atan(x,y)", "atan2(y,x)");


		Regex open = new Regex(@"mat(.)\s*(\()");
		Match match = open.Match(code);
		while (match.Success){
			Group g = match.Groups[2];
			CaptureCollection cc = g.Captures;
		    Capture c = cc[0];
			int startPos = c.Index;
			int openBr = 1;
			int index = 1;

			do{
				char nextChar = code[startPos + index];
				if (nextChar == '(')
					openBr++;
				else if (nextChar == ')')
					openBr--;
				index++;
			}while(openBr > 0);
	
			string matString = code.Substring(startPos, index);

			string matStringPattern = matString.Replace("(", @"\(");
			matStringPattern = matStringPattern.Replace(")", @"\)");
			code = Regex.Replace(code, @"mat(.)\s*" + matStringPattern + @"\s*\*(\s*\w+)", "mul(float$1x$1" + matString + ",$2)");

			match = match.NextMatch();
		}
		
		File.WriteAllText(path, code);
	}

	[MenuItem (MenuRoot + "ShaderToy->Unity", true)]
	static bool ValidateShaderFile() {
		string path = AssetDatabase.GetAssetPath (Selection.activeObject);
		string ext = Path.GetExtension(path);
		return (ext == ".shader");
	}

}
