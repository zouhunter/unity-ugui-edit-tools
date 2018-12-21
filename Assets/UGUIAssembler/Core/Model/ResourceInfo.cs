using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UGUIAssembler
{
    public class ResourceDic : Dictionary<string,string>
    {
        public bool active = true;
        public ResourceDic() { }
        public ResourceDic(ResourceDic info) : base(info) { }

    }
}