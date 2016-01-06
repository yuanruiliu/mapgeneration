Shader "InvisibleWithShadow"
{
    Subshader
    {
       UsePass "VertexLit/SHADOWCOLLECTOR"    
       UsePass "VertexLit/SHADOWCASTER"
    }
 
    Fallback off
}