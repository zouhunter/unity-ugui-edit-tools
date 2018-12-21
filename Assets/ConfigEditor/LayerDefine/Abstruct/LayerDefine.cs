using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace UGUIAssembler.Config
{
    public interface ILayerDefine
    {
        /// <summary>
        /// 类型
        /// </summary>
        Type type { get; }
        /// <summary>
        /// 所有支持的元素
        /// </summary>
        ICollection<string> supportMembers { get; }

        /// <summary>
        /// 元素类型字典
        /// </summary>
        Type GetMemberType(string key);
        
        /// <summary>
        /// 子元素的控件类型
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        string GetSubControlType(string key);

        /// <summary>
        /// 子元素名称
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        string GetSubControlName(string key);

        /// <summary>
        /// 必须支持的子控件
        /// </summary>
        List<string> integrantSubControls { get; }

        /// <summary>
        /// 支持的子控件
        /// </summary>
        List<string> runtimeSubControls { get; }
    }
}
