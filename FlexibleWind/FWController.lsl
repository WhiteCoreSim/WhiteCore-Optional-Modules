//
// A Simple Fluid Solver Wind Controller v1.0.1
//     
//          by Fumi.Iseki
//
// Updated for the FlexibleWind module for WhiteCore
//  20160215 - greythane -
//


integer DEFAULT_CHANNEL = 6000;

integer cmd_channel = 0;
integer listen_hdl  = 0;



init()
{
    cmd_channel  = DEFAULT_CHANNEL;
 
    if (listen_hdl!=0) llListenRemove(listen_hdl);
    listen_hdl = llListen(cmd_channel, "", NULL_KEY, "");
}


//
// Default State
// 
default
{
    listen(integer ch, string name, key id, string msg) 
    {
        if (listen_hdl!=0) llListenRemove(listen_hdl);
        llSay(0, "Recived: " + msg);

        list items = llParseString2List(msg, ["=", ",", " ","\n"], []);
        string cmd = llList2String(items, 0);
        
        if (cmd=="strength") {
            string param = llList2String(items, 1);
            osSetWindParam("FlexibleWind", "strength", (float)param);
            llSay(0, "set parameter : "+cmd+" = "+param);
        }

        else if (cmd=="damping") {
            string param = llList2String(items, 1);
            osSetWindParam("FlexibleWind", "damping", (float)param);
            llSay(0, "set parameter : "+cmd+" = "+param);
        }

        else if (cmd=="direction") {
            string param = llList2String(items, 1);
            osSetWindParam("FlexibleWind", "force", (float)param);
            llSay(0, "set parameter : "+cmd+" = "+param);
        }
        
        else if (cmd=="period") {
            string param = llList2String(items, 1);
            osSetWindParam("FlexibleWind", "period", (float)param);
            llSay(0, "set parameter : "+cmd+" = "+param);
        }

        else if (cmd=="viscosity") {
            string param = llList2String(items, 1);
            osSetWindParam("FlexibleWind", "viscosity", (float)param);
            llSay(0, "set parameter : "+cmd+" = "+param);
        }

        else if (cmd=="variationrate") {
            string param = llList2String(items, 1);
            osSetWindParam("FlexibleWind", "variationrate", (float)param);
            llSay(0, "set parameter : "+cmd+" = "+param);
        }
        
        else if (cmd=="stop") {
            osSetWindParam("FlexibleWind", "stop", 0.0);
            llSay(0, "stop the wind");
        }

        else if (cmd=="reset") {
            osSetWindParam("FlexibleWind", "stop", 0.0);
            llSay(0, "reset the controller");
            init();
        }

        else if (cmd=="help") {
            llSay(0, "/"+(string)cmd_channel+" direction number");
            llSay(0, "/"+(string)cmd_channel+" strength number");
            llSay(0, "/"+(string)cmd_channel+" damping number");
            llSay(0, "/"+(string)cmd_channel+" period number");
            llSay(0, "/"+(string)cmd_channel+" viscosity number");
            llSay(0, "/"+(string)cmd_channel+" variationrate number");
            llSay(0, "/"+(string)cmd_channel+" stop");
            llSay(0, "/"+(string)cmd_channel+" reset");
            llSay(0, "/"+(string)cmd_channel+" help");
        }

        else {
            llSay(0, "Unknown Command: " + cmd);
        }
        
        if (listen_hdl!=0) llListenRemove(listen_hdl);
        listen_hdl = llListen(cmd_channel, "", NULL_KEY, "");
    }


    changed(integer change)
    {
        if (change & CHANGED_REGION_START) {
            llResetScript();
        }       
    }


    state_entry()
    {
        init();
    }


    on_rez(integer start_param) 
    {
        llResetScript();
    }


    touch_start(integer total_number)
    {
        llSay(0, "Direction = "+(string)((integer)osGetWindParam("FlexibleWind", "direction")));
        llSay(0, "Force Strength = "+(string)osGetWindParam("FlexibleWind", "strength"));
        llSay(0, "Force Damping Rate = "+(string)osGetWindParam("FlexibleWind", "damping"));
        llSay(0, "Force Period = "+(string)((integer)osGetWindParam("FlexibleWind", "period")));
        llSay(0, "Wind Viscosity = "+(string)(osGetWindParam("FlexibleWind", "viscosity")));
        llSay(0, "Wind Variation Rate = "+(string)(osGetWindParam("FlexibleWind", "variationrate")));
        llSay(0, "Command Channel = "+(string)cmd_channel);
    }
}
