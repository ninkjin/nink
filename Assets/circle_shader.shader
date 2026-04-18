Shader "Custom/PerspectiveMask"
{
    Properties
    {
        _MainColor ("遮罩颜色（默认黑色）", Color) = (0,0,0,1) // 可在Inspector中调整遮罩颜色
        _CircleSoftness ("圆形边缘柔和度", Range(0, 0.1)) = 0.02 // 控制透视边缘模糊程度
    }
    SubShader
    {
        // 渲染队列设为透明层，确保在背景之上、UI之下（可根据需求调整）
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        LOD 100

        // 第一遍Pass：渲染圆形区域，仅写入模板缓冲区（不显示颜色）
        Pass
        {
            Stencil
            {
                Ref 1 // 模板参考值（标记圆形区域）
                Comp Always // 总是通过测试
                Pass Replace // 将模板缓冲区的值替换为参考值1
            }

            ColorMask 0 // 不写入颜色缓冲区（只标记模板，圆形本身不显示）
            ZWrite Off // 关闭深度写入，避免遮挡其他物体
            Cull Off // 关闭背面剔除（确保圆形全角度可见）

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION; // 顶点位置
                float2 uv : TEXCOORD0; // UV坐标（用于计算圆形范围）
            };

            struct v2f
            {
                float4 pos : SV_POSITION; // 裁剪空间位置
                float2 uv : TEXCOORD0; // 传递UV给片段着色器
            };

            // 圆形的UV范围（默认Sprite的UV是0-1，中心在(0.5,0.5)）
            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex); // 转换顶点到裁剪空间
                o.uv = v.vertex.xy * 0.5 + 0.5; // 将顶点坐标转换为0-1的UV（适配Sprite）
                return o;
            }

            float _CircleSoftness;

            fixed4 frag (v2f i) : SV_Target
            {
                // 计算当前像素到圆形中心的距离（UV中心是(0.5,0.5)）
                float2 center = float2(0.5, 0.5);
                float distance = length(i.uv - center);

                // 圆形半径设为0.5（刚好覆盖UV范围），边缘通过softness实现模糊
                float circle = smoothstep(0.5, 0.5 - _CircleSoftness, distance);

                // 只保留圆形区域的模板标记（非圆形区域不写入模板）
                return circle;
            }
            ENDCG
        }

        // 第二遍Pass：渲染黑色遮罩，仅在非圆形区域显示
        Pass
        {
            Stencil
            {
                Ref 1 // 参考值（与第一遍Pass一致）
                Comp NotEqual // 只渲染模板值≠1的区域（即非圆形区域）
            }

            ZWrite Off // 关闭深度写入，避免遮挡底层背景
            Blend SrcAlpha OneMinusSrcAlpha // 启用alpha混合（支持半透明遮罩）
            Cull Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
            };

            float4 _MainColor;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // 输出遮罩颜色（默认黑色）
                return _MainColor;
            }
            ENDCG
        }
    }
}
