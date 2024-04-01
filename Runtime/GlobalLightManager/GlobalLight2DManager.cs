using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace GoldSprite.UnityPlugins.DayNightCycleSystem {
    /// <summary>
    /// ����ϵͳʱ��Ӹ�����GameTimeLoop��λ(Ĭ��3����=24��Ϸʱ)���ع�һ������ǿ��
    /// </summary>
    public class GlobalLight2DManager : MonoBehaviour {
        [Header("����")]
        public Light2D globalLight;

        [Header("����")]
        [Tooltip("gameMinPerDay��ʾÿ��ʵʱ��{0}����Ϊ��Ϸ��24h.")]
        public float gameMinPerDay = 5;
        [Range(0, 1)]
        public float lightingRange_min = 0.3f;
        [Range(0, 1)]
        public float lightingRange_max = 1;
        public bool manualMode;
        [Range(0, 24)]
        public float manualGameTimeHours;
        public Color dayColor = ColorTools.HexToColor("FFC2C2");
        public Color nightColor = ColorTools.HexToColor("9999FF");

        [Header("ʵʱ")]
        public double SystemTimeSeconds;
        [Header("ϵͳ��ǰʱ��")]
        public string SystemTime;
        public double currentSystemMins;
        [Header("��Ϸ��һ����ǰʱ��")]
        public double gameTimeNormalized;
        public float gameTimeHours;
        [Header("��Ϸʱ��")]
        public string gameTime;
        [Range(0, 1)]
        [Header("��ǰ��һ������ǿ��")]
        public float currentLightingNormalized;
        [Range(0, 1)]
        [Header("��ǰ����ǿ��")]
        public float currentLighting;
        [Header("��ǰ������ɫƫ��")]
        public Color clightColor;
        public Color cDayColor;
        public Color cNightColor;
        public List<Light2D> environmentLights => FindObjectsOfType<Light2D>().Where(p=>p!= globalLight).ToList();

        void Start()
        {
            globalLight = GetComponent<Light2D>();
        }

        void Update()
        {
            //���㵱ǰϵͳ������
            SystemTimeSeconds = DateTime.Now.TimeOfDay.TotalSeconds;
            SystemTime = new DateTime((long)(SystemTimeSeconds * TimeSpan.TicksPerSecond)).ToString("HH:mm:ss");
            currentSystemMins = SystemTimeSeconds / 60f;

            //���㵱ǰ��Ϸʱ
            gameTimeNormalized = currentSystemMins % gameMinPerDay / gameMinPerDay;
            gameTimeHours = manualMode ? manualGameTimeHours : (float)(gameTimeNormalized * 24);
            gameTime = new DateTime((long)(gameTimeHours * TimeSpan.TicksPerHour)).ToString("HH:mm:ss");

            //�������/������ǿ��
            currentLightingNormalized = GetDayTimeLighting(gameTimeHours);
            currentLighting = LimitLightingRange(currentLightingNormalized);
            clightColor = GetLightingColor(currentLightingNormalized);


            globalLight.intensity = currentLighting;
            globalLight.color = clightColor;

            //������Դ
            foreach(var light in environmentLights) {
                light.intensity = 1 - currentLighting;
            }
        }

        //0/24����������ǿ, 12�����������ǿ
        private Color GetLightingColor(float lighting)
        {
            var sunIntensity = lighting;
            var moonIntensity = 1 - lighting;

            cDayColor = dayColor * sunIntensity; cDayColor.a = 1;
            cNightColor = nightColor * moonIntensity; cNightColor.a = 1;

            clightColor = Color.Lerp(cNightColor, cDayColor, lighting);
            return clightColor;
        }

        public float GetDayTimeLighting(float hours)
        {
            var normalizeHours = hours / 24f;
            var rad = normalizeHours * Math.PI * 2;
            var cos = Math.Cos(rad);
            var result = (float)(-cos / 2f + 0.5f);
            return result;
        }
        public float LimitLightingRange(double lighting)
        {
            var range = lightingRange_max - lightingRange_min;
            var result = (float)(lightingRange_min + lighting * range);
            return result;
        }
    }


    public class ColorTools
    {
        public static Color GetColorByHexString(string hexString)
        {
            ColorUtility.TryParseHtmlString(hexString, out var color);
            return color;
        }

        public static Color HexToColor(string hex)
        {
            // ��ʮ�������ַ�������Ϊ�졢�̡���������ɫ��ֵ
            float r = (float)System.Convert.ToInt32(hex.Substring(0, 2), 16) / 255f;
            float g = (float)System.Convert.ToInt32(hex.Substring(2, 2), 16) / 255f;
            float b = (float)System.Convert.ToInt32(hex.Substring(4, 2), 16) / 255f;

            // ����Color�ṹ������
            return new Color(r, g, b);
        }
    }


}
