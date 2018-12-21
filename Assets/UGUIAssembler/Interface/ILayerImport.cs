using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace UGUIAssembler
{
    public interface ILayerImport
    {
        AssemblerStateMechine mechine { get; set; }
        UINode DrawLayer(LayerInfo layer);
    }

    public interface IParamsSetter
    {
        void SetUIParams(UINode node);
    }

    public interface IFinalCheckUp
    {
        void FinalCheckUp(UINode node);
    }
}