using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IlluminationQuizz : MonoBehaviour
{

    Vector3[] ApplyTransform(Matrix4x4 m, Vector3[] verts){
        int num = verts.Length;
        Vector3[] result = new Vector3[num];
        for(int i=0; i<num; i++){
            Vector3 v = verts[i];
            Vector4 temp = new Vector4(v.x,v.y,v.z,1);
            result[i] = m* temp;
        }

        return result;
    }


    Dictionary<string, Vector3> vectors;
    Dictionary<string, float> scalars;
    Vector3[] originalCoordinates;
    GameObject sphere;
    float textureU;
    float textureV; 
    Vector3 textureColor;
    Vector3 finalColor;


    // Start is called before the first frame update
    void Start()
    {
        Vector4 A = new Vector4(1.41108f, -1.02919f, -0.30249f,1);
        Matrix4x4 AP = Transformations.TranslateM(0.45310f, -0.76659f, -0.95688f);
        Vector3 Pivot = new Vector3(0.02739f, -0.69375f, -0.00425f);
        Matrix4x4 nP = Transformations.TranslateM(-Pivot.x, -Pivot.y, -Pivot.z);
        Matrix4x4 R = Transformations.RotateM(-1.4f, Transformations.AXIS.AX_X);
        Matrix4x4 sP= Transformations.TranslateM(Pivot.x, Pivot.y, Pivot.z);

        vectors = new Dictionary<string, Vector3>();
        vectors.Add("SC", AP * nP * R * sP * A);
        
        vectors.Add("ka", new Vector3(0.16857f, 0.14195f, 0.18014f ));
        vectors.Add("kd", new Vector3(0.60667f, 0.01357f, 0.19082f )); 
        vectors.Add("ks", new Vector3(0.35484f, 0.13845f, 0.32000f ));
        vectors.Add("Ia", new Vector3(0.83647f, 0.86650f, 0.91773f ));
        vectors.Add("Id", new Vector3(0.99706f, 0.89655f, 0.80751f ));
        vectors.Add("Is", new Vector3(0.80964f, 0.82526f, 0.96783f ));
        
        vectors.Add("LIGHT", new Vector3(3.03803f, -3.20971f, -3.50541f));
        vectors.Add("CAM", new Vector3(3.93795f, -3.66019f,  1.60283f));

        Vector3 PoI = Math3D.SphericalToCartesian(141, 100, 0.92440f, vectors["SC"]);
        vectors.Add("PoI", PoI);

        vectors.Add("n", vectors["PoI"] - vectors["SC"]);
        vectors.Add("nUnit", Math3D.Normalize(vectors["n"]));
        vectors.Add("l", vectors["LIGHT"] - vectors["PoI"]);
        vectors.Add("lUnit", Math3D.Normalize(vectors["l"]));
        vectors.Add("v", vectors["CAM"] - vectors["PoI"]);
        vectors.Add("vUnit", Math3D.Normalize(vectors["v"]));
        vectors.Add("r", Math3D.Reflect(vectors["n"],vectors["l"]));
        vectors.Add("rUnit", Math3D.Normalize(vectors["r"]));

        scalars = new Dictionary<string, float>();
        scalars.Add("alpha", 134.0f);
        scalars.Add("lDotN", Math3D.Dot(vectors["nUnit"], vectors["lUnit"]));
        scalars.Add("vDotR", Math3D.Dot(vectors["vUnit"], vectors["rUnit"]));
        scalars.Add("vDotRAlpha", Mathf.Pow(scalars["vDotR"], scalars["alpha"]));

        Vector3 color = Vector3.zero;
        color.x = vectors["ka"].x * vectors["Ia"].x +
                  vectors["kd"].x * vectors["Id"].x * scalars["lDotN"] +
                  vectors["ks"].x * vectors["Is"].x * scalars["vDotRAlpha"];

        color.y = vectors["ka"].y * vectors["Ia"].y +
                  vectors["kd"].y * vectors["Id"].y * scalars["lDotN"] +
                  vectors["ks"].y * vectors["Is"].y * scalars["vDotRAlpha"];

        color.z = vectors["ka"].z * vectors["Ia"].z +
                  vectors["kd"].z * vectors["Id"].z * scalars["lDotN"] +
                  vectors["ks"].z * vectors["Is"].z * scalars["vDotRAlpha"];

        vectors.Add("COLOR", color);
        Debug.Log("Color: " + vectors["COLOR"]);

        //  foreach (KeyValuePair<string, Vector3> par in vectors)
        // {
        //     Debug.Log(par.Key + ": " + par.Value.ToString("F5"));
        // }

        //   foreach (KeyValuePair<string, float> par in scalars)
        // {
        //     Debug.Log(par.Key + ": " + par.Value.ToString("F5"));
        // }

        // Texture
        textureU = 0.5f + (Mathf.Asin(vectors["nUnit"].x))/Mathf.PI;
        textureV = 0.5f + (Mathf.Asin(vectors["nUnit"].y))/Mathf.PI;

        Debug.Log("U: " + textureU); //0.7127699  
        Debug.Log("V: " + textureV); //0.2166667  

            // 0.8000 - 0.9999          A                 B                 C                 D                 E  
            // 0.6000 - 0.7999          F                 G                 H                 I                 J  
            // 0.4000 - 0.5999          K                 L                 M                 N                 O  
            // 0.2000 - 0.3999          P                 Q                 R                 S                 T  
            // 0.0000 - 0.1999          U                 V                 W                 X                 Y  
            //                   0.0000 - 0.1999   0.2000 - 0.3999   0.4000 - 0.5999   0.6000 - 0.7999   0.8000 - 0.9999

    
        // Cell: S
        textureColor = new Vector3(0.65973f,   0.83702f,   0.62154f);
        finalColor = new Vector3(vectors["COLOR"].x * textureColor.x,
                                 vectors["COLOR"].y * textureColor.y,
                                 vectors["COLOR"].z * textureColor.z);

        Debug.Log("FinalColor : " + finalColor.x.ToString("F5") + "  " +finalColor.y.ToString("F5") + "  " + finalColor.z.ToString("F5") ); 

        sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.GetComponent<MeshRenderer>().material.SetColor("_Color", new Color(finalColor.x,finalColor.y,finalColor.z));    
        originalCoordinates = sphere.GetComponent<MeshFilter>().mesh.vertices;

        Matrix4x4 ST = Transformations.TranslateM(
        vectors["SC"].x, 
        vectors["SC"].y, 
        vectors["SC"].z);

        Matrix4x4 SS = Transformations.ScaleM(
            0.92440f,
            0.92440f,
            0.92440f
        );

        sphere.GetComponent<MeshFilter>().mesh.vertices = ApplyTransform(ST*SS, originalCoordinates); 
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
