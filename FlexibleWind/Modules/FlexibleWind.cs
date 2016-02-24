/*
 * Copyright (c) Contributors, http://whitecore-sim.org/, Fumi.Iseki
 *
 * A Simple Fluid Solver Wind Module for OpenSim, upgraded for WhiteCoreSim
 * using AForge.Math for calculations
 *
 * See CONTRIBUTORS.TXT for a full list of copyright holders.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 *     * Redistributions of source code must retain the above copyright
 *       notice, this list of conditions and the following disclaimer.
 *     * Redistributions in binary form must reproduce the above copyright
 *       notice, this list of conditions and the following disclaimer in the
 *       documentation and/or other materials provided with the distribution.
 *     * Neither the name of the Aurora-Sim Project nor the
 *       names of its contributors may be used to endorse or promote products
 *       derived from this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE DEVELOPERS ``AS IS'' AND ANY
 * EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL THE CONTRIBUTORS BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

using System;
using System.Collections.Generic;
using Nini.Config;
using OpenMetaverse;
using WhiteCore.Framework.ConsoleFramework;
using WhiteCore.Framework.Modules;
using WhiteCore.Framework.SceneInfo;

namespace WhiteCore.Addon.FlexibleWind
{
    public class FlexibleWind : IWindModelPlugin
    {
        const int m_mesh = 16;
       
        int m_wind_direction = 0;           // The desirecd wind direction (external force)
        //  0 : Random Wind (default)
        //  1 : North Wind
        //  2 : East Wind
        //  3 : South Wind
        //  4 : West Wind
        //  5 : Rotational Wind
        float m_damping_rate = 0.85f;       // Damping rate of the external force
        float m_viscosity = 0.001f;         // Viscosity coefficient of the wind
        int m_region_size = 256;

        float m_energy_variation = 0.004f;        // Lower limit of the energy variation rate
        float m_energy = 0.0f;
        int m_energy_cnt = 0;

        int m_period = 0;                   // Period of external the external force
        int m_period_cnt = 0;

        Vector2[] m_windSpeeds;

        float m_strength = 1.0f;
        Random m_rndnums;

        float[] m_windSpeeds_u;
        float[] m_windSpeeds_v;
        float[] m_windForces_u;
        float[] m_windForces_v;
        float[] m_extrForces_u;
        float[] m_extrForces_v;

        float[] m_work_u;
        float[] m_work_v;

        AForge.Math.Complex[,] m_comp_u;
        AForge.Math.Complex[,] m_comp_v;


        #region IWindModulePlugin Members

        public string Version
        {
            get { return "1.0.1.0"; }
        }

        public string Name
        {
            get { return "FlexibleWind"; }
        }


        public void Initialise ()
        {
            m_rndnums = new Random (Environment.TickCount);

            m_windSpeeds_u = new float[m_mesh * m_mesh];
            m_windSpeeds_v = new float[m_mesh * m_mesh];
            m_windForces_u = new float[m_mesh * m_mesh];
            m_windForces_v = new float[m_mesh * m_mesh];
            m_extrForces_u = new float[m_mesh * m_mesh];
            m_extrForces_v = new float[m_mesh * m_mesh];
            //
            m_windSpeeds = new Vector2[m_mesh * m_mesh];

            m_work_u = new float[m_mesh * m_mesh];
            m_work_v = new float[m_mesh * m_mesh];

            m_comp_u = new AForge.Math.Complex[m_mesh, m_mesh];
            m_comp_v = new AForge.Math.Complex[m_mesh, m_mesh];

            clearForces ();
            clearSpeeds ();
            addForces (m_wind_direction);
        }

        public void WindConfig (IScene scene, IConfig windConfig)
        {
            if (scene == null)
                return;

            if (windConfig != null)
            {
                m_strength = windConfig.GetFloat ("strength", m_strength);
                m_damping_rate = windConfig.GetFloat ("damping", m_damping_rate);
                if (m_damping_rate > 1.0f)
                    m_damping_rate = 1.0f;
                
                m_wind_direction = windConfig.GetInt ("direction", m_wind_direction);
                if (m_wind_direction < 0)
                    m_wind_direction = 0;
                if (m_wind_direction > 5)
                    m_wind_direction = 5;

                m_period = windConfig.GetInt ("period", m_period);
                if (m_period < 0)
                    m_period = 0;

                m_energy_variation = windConfig.GetFloat ("variationrate", m_energy_variation);
                if (m_energy_variation <= 0.0f)
                    m_energy_variation = 0.001f;

                m_viscosity = windConfig.GetFloat ("viscosity", m_viscosity);
                if (m_viscosity < 0.0f)
                    m_viscosity = 0.001f;

                // always using full region size (This does assume square regions)
                m_region_size = scene.RegionInfo.RegionSizeX;

            }
        }


        public void WindUpdate (uint frame)
        {
            if (m_windSpeeds != null)
            {
                for (int i = 0; i < m_mesh * m_mesh; i++)
                {
                    m_windForces_u [i] = m_extrForces_u [i] * m_strength;
                    m_windForces_v [i] = m_extrForces_v [i] * m_strength;
                }

                SolveSFSW (m_mesh, m_windSpeeds_u, m_windSpeeds_v, m_windForces_u, m_windForces_v, m_region_size, m_viscosity, 1.0f);

                float energy = 0.0f;
                for (int i = 0; i < m_mesh * m_mesh; i++)
                {
                    m_windSpeeds [i].X = m_windSpeeds_u [i];
                    m_windSpeeds [i].Y = m_windSpeeds_v [i];
                    //
                    m_extrForces_u [i] *= m_damping_rate;
                    m_extrForces_v [i] *= m_damping_rate;

                    energy += m_windSpeeds_u [i] * m_windSpeeds_u [i] + m_windSpeeds_v [i] * m_windSpeeds_v [i];
                }
                //
                if (energy != 0.0f)
                {
                    float st_rate = (m_energy - energy) / energy;
                    //
                    if (st_rate >= 0.0f && st_rate <= m_energy_variation)
                    {
                        m_energy_cnt++;
                        if (m_energy_cnt > 5)
                        {
                            MainConsole.Instance.InfoFormat ("[FlexibleWind]: Restart Wind by Energy Limit.");
                            // restart wind
                            clearForces ();
                            clearSpeeds ();
                            addForces (m_wind_direction);
                            m_period_cnt = 0;
                            m_energy_cnt = 0;
                        }
                    } else
                        m_energy_cnt = 0;
                } else
                    m_energy_cnt = 0;

                m_energy = energy;
                //
                if (m_period != 0)
                {
                    m_period_cnt++;
                    if (m_period_cnt >= m_period)
                    {
                        MainConsole.Instance.InfoFormat ("[FlexibleWind]: Restart Wind by Period.");
                        // restart wind
                        clearForces ();
                        clearSpeeds ();
                        addForces (m_wind_direction);
                        m_period_cnt = 0;
                        m_energy_cnt = 0;
                    }
                }
            }
        }


        public Vector3 WindSpeed (float fX, float fY, float fZ)
        {
            Vector3 windVector = new Vector3 (0.0f, 0.0f, 0.0f);

            if (m_windSpeeds != null)
            {
                int x = (int)fX / m_mesh;
                int y = (int)fY / m_mesh;

                if (x < 0)
                    x = 0;
                if (x > m_mesh - 1)
                    x = m_mesh - 1;
                if (y < 0)
                    y = 0;
                if (y > m_mesh - 1)
                    y = m_mesh - 1;

                windVector.X = m_windSpeeds [y * m_mesh + x].X;
                windVector.Y = m_windSpeeds [y * m_mesh + x].Y;
            }

            return windVector;
        }


        public Vector2[] WindLLClientArray ()
        {
            return m_windSpeeds;
        }


        public string Description
        {
            get
            {
                return "Provides a simple fluid solver wind by Jos Stam."; 
            }
        }


        public Dictionary<string, string> WindParams ()
        {
            Dictionary<string, string> Params = new Dictionary<string, string> ();

            Params.Add ("direction", "Kind of the external force");
            Params.Add ("period", "Period of the external force");
            Params.Add ("strength", "Wind strength");
            Params.Add ("damping", "Damping rate of the external force");
            Params.Add ("viscosity", "Viscosity coefficient of the wind");
            Params.Add ("variationrate", "Lower limit of the energy variation rate");
            Params.Add ("stop", "Stop the wind");

            return Params;
        }


        public void WindParamSet (string param, float value)
        {
            switch (param)
            {
            case "direction":
                m_wind_direction = (int)value;
                if (m_wind_direction < 0)
                    m_wind_direction = 0;
                MainConsole.Instance.InfoFormat ("[FlexibleWind]: Set Param : force = {0}", m_wind_direction);
                clearForces ();
                addForces (m_wind_direction);
                break;

            case "strength":
                m_strength = value;
                MainConsole.Instance.InfoFormat ("[FlexibleWind]: Set Param : strength = {0}", m_strength);
                break;

            case "damping":
                m_damping_rate = value;
                if (m_damping_rate > 1.0f)
                    m_damping_rate = 1.0f;
                MainConsole.Instance.InfoFormat ("[FlexibleWind]: Set Param : damping = {0}", m_damping_rate);
                break;

            case "period":
                m_period = (int)value;
                if (m_period < 0)
                    m_period = 0;
                MainConsole.Instance.InfoFormat ("[FlexibleWind]: Set Param : period = {0}", m_period);
                m_period_cnt = 0;
                break;

            case "viscosity":
                m_viscosity = value;
                if (m_viscosity < 0.0f)
                    m_viscosity = 0.001f;
                MainConsole.Instance.InfoFormat ("[FlexibleWind]: Set Param : wind_visc = {0}", m_viscosity);
                break;

            case "variationrate":
                m_energy_variation = value;
                if (m_energy_variation <= 0.0f)
                    m_energy_variation = 0.001f;
                MainConsole.Instance.InfoFormat ("[FlexibleWind]: Set Param : wind_eps = {0}", m_energy_variation);
                m_energy_cnt = 0;
                break;

            case "stop":
                MainConsole.Instance.InfoFormat ("[FlexibleWind]: Command : stop");
                clearForces ();
                clearSpeeds ();
                break;
            }
        }


        public float WindParamGet (string param)
        {
            switch (param)
            {
            case "direction":
                return (float)m_wind_direction;

            case "strength":
                return m_strength;

            case "damping":
                return m_damping_rate;

            case "period":
                return (float)m_period;

            case "wind_visc":
                return m_viscosity;

            case "wind_eps":
                return m_energy_variation;

            default:
                {
                    MainConsole.Instance.WarnFormat ("[Flexible Wind]: Unknown {0} parameter {1}", Name, param);
                    return 0;
                }
            }
        }


        #endregion

        #region IDisposable Members

        public void Dispose ()
        {
            m_windSpeeds = null;
            //
            m_windSpeeds_u = null;
            m_windSpeeds_v = null;
            m_windForces_u = null;
            m_windForces_v = null;
            m_extrForces_u = null;
            m_extrForces_v = null;

            m_work_u = null;
            m_work_v = null;
            //
            m_comp_u = null;
            m_comp_v = null;
        }

        #endregion


        public void clearSpeeds ()
        {
            if (m_windSpeeds != null)
            {
                for (int i = 0; i < m_mesh * m_mesh; i++)
                {
                    m_windSpeeds [i].X = 0.0f;
                    m_windSpeeds [i].Y = 0.0f;
                }
            }

            if (m_windSpeeds_u != null && m_windSpeeds_v != null)
            {
                for (int i = 0; i < m_mesh * m_mesh; i++)
                {
                    m_windSpeeds_u [i] = 0.0f;
                    m_windSpeeds_v [i] = 0.0f;
                }
            }
        }


        public void clearForces ()
        {
            if (m_extrForces_u != null && m_extrForces_v != null)
            {
                for (int i = 0; i < m_mesh * m_mesh; i++)
                {
                    m_extrForces_u [i] = 0.0f; 
                    m_extrForces_v [i] = 0.0f; 
                }
            }
        }


        void addForces (int wind_direction)
        {
            if (m_extrForces_u != null && m_extrForces_v != null)
            {
                int i, j;


                switch (wind_direction)
                {
                case 0:     // random
                    for (i = 0; i < m_mesh * m_mesh; i++)
                    {
                        m_extrForces_u [i] = (float)(m_rndnums.NextDouble () * 2d - 1d);    // -1 to 1 
                        m_extrForces_v [i] = (float)(m_rndnums.NextDouble () * 2d - 1d);    // -1 to 1 
                    }
                    break;
                case 1:     // North
                    for (i = m_mesh / 3; i < m_mesh - m_mesh / 3; i++)
                    {
                        m_extrForces_v [i + (m_mesh - 2) * m_mesh] -= 2.0f;
                    }
                    break;
                case 2:     //East
                    for (j = m_mesh / 3; j < m_mesh - m_mesh / 3; j++)
                    {
                        m_extrForces_u [m_mesh - 2 + j * m_mesh] -= 2.0f;
                    }
                    break;
                case 3:     // South
                    for (i = m_mesh / 3; i < m_mesh - m_mesh / 3; i++)
                    {
                        m_extrForces_v [i + m_mesh] += 2.0f;
                    }
                    break;
                case 4:     // West
                    for (j = m_mesh / 3; j < m_mesh - m_mesh / 3; j++)
                    {
                        m_extrForces_u [1 + j * m_mesh] += 2.0f;
                    }
                    break;
                case 5:     // Rotational
                    float radius = ((float)m_mesh) / 6.0f;
                    for (float f = 0.0f; f < (float)Math.PI; f += 0.01f)
                    {
                        float angle = 2.0f * f;
                        float x = (float)Math.Cos (angle) * radius;
                        float y = (float)Math.Sin (angle) * radius;
                        //
                        m_extrForces_u [(int)(m_mesh / 2 + x) + (int)(m_mesh / 2 + y) * m_mesh] -= (float)Math.Sin (angle) * 0.2f;
                        m_extrForces_v [(int)(m_mesh / 2 + x) + (int)(m_mesh / 2 + y) * m_mesh] += (float)Math.Cos (angle) * 0.2f;
                    }
                    break;
                }
            }

            return;
        }



        /////////////////////////////////////////////////////////////////////////////////////////////
        // Original Simple Fluid Solver Code is by Jos Stam
        // 	  http://www.dgp.utoronto.ca/people/stam/reality/Research/pub.html
        //
        //    Using AForge.Math.FourierTransform for FFT
        //

        void SolveSFSW (int n, float[] wu, float[] wv, float[] fu, float[] fv, int rsize, float visc, float dt)
        {
            for (int i = 0; i < n * n; i++)
            {
                wu [i] += dt * fu [i];
                wv [i] += dt * fv [i];
                m_work_u [i] = wu [i] / rsize;
                m_work_v [i] = wv [i] / rsize;
            }

            for (int j = 0; j < n; j++)
            {
                int jj = j * n;
                for (int i = 0; i < n; i++)
                {
                    float x = i - dt * m_work_u [i + jj] * n;
                    float y = j - dt * m_work_v [i + jj] * n;
                    //
                    int xi = (int)Math.Floor (x);
                    int yi = (int)Math.Floor (y);
                    float s = (x - xi);
                    float t = (y - yi);

                    int i0 = (n + (xi % n)) % n;
                    int i1 = (i0 + 1) % n;
                    int j0 = (n + (yi % n)) % n;
                    int j1 = (j0 + 1) % n;

                    wu [i + jj] = (1.0f - s) * ((1.0f - t) * m_work_u [i0 + n * j0] + t * m_work_u [i0 + n * j1])
                                    + s * ((1.0f - t) * m_work_u [i1 + n * j0] + t * m_work_u [i1 + n * j1]);
                    wv [i + jj] = (1.0f - s) * ((1.0f - t) * m_work_v [i0 + n * j0] + t * m_work_v [i0 + n * j1])
                                    + s * ((1.0f - t) * m_work_v [i1 + n * j0] + t * m_work_v [i1 + n * j1]);
                }
            }

            for (int j = 0; j < n; j++)
            {
                int jj = j * n;
                for (int i = 0; i < n; i++)
                {
                    m_comp_u [i, j] = new AForge.Math.Complex (wu [i + jj], 0.0);
                    m_comp_v [i, j] = new AForge.Math.Complex (wv [i + jj], 0.0);
                }
            }

            AForge.Math.FourierTransform.FFT2 (m_comp_u, AForge.Math.FourierTransform.Direction.Forward);
            AForge.Math.FourierTransform.FFT2 (m_comp_v, AForge.Math.FourierTransform.Direction.Forward);

            visc *= dt;
            for (int j = 0; j < n; j++)
            {
                float y = (float)(j <= n / 2 ? j : j - n);
                float yy = y * y;
                //
                for (int i = 0; i < n / 2 + 1; i++)
                {
                    float xx = i * i;
                    float r_sq = xx + yy;
                    if (r_sq != 0.0f)
                    {
                        float fac = (float)Math.Exp (-r_sq * visc);
                        float xy = i * y;

                        AForge.Math.Complex ux = m_comp_u [i, j];
                        AForge.Math.Complex vx = m_comp_v [i, j];
                        m_comp_u [i, j] = fac * ((1.0f - xx / r_sq) * ux - xy / r_sq * vx);
                        m_comp_v [i, j] = fac * (-xy / r_sq * ux + (1.0f - yy / r_sq) * vx);
                    }
                }
            }

            AForge.Math.FourierTransform.FFT2 (m_comp_u, AForge.Math.FourierTransform.Direction.Backward);
            AForge.Math.FourierTransform.FFT2 (m_comp_v, AForge.Math.FourierTransform.Direction.Backward);

            float nrml = 1.0f;//(n*n);
            for (int j = 0; j < n; j++)
            {
                int jj = j * n;
                for (int i = 0; i < n; i++)
                {
                    wu [i + jj] = (float)(nrml * m_comp_u [i, j].Re) * rsize;
                    wv [i + jj] = (float)(nrml * m_comp_v [i, j].Re) * rsize;
                }
            }

            return;
        }

    }
}
