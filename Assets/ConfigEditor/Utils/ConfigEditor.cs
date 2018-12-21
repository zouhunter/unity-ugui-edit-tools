using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UGUIAssembler;
using System.IO;
using System.Linq;
using Ntreev.Library.Psd;

namespace UGUIAssembler.Config
{
    public class ConfigEditor : MonoBehaviour
    {
        [SerializeField] private Button m_import;
        [SerializeField] private Button m_renew;
        [SerializeField] private Button m_psdImport;
        [SerializeField] private Button m_export;
        [SerializeField] private InputField m_spriteFolder;
        [SerializeField] private InputField m_textureFolder;
        [SerializeField] private Canvas targetCanvas;
        [SerializeField] private Button m_spriteFolderBtn;
        [SerializeField] private Button m_textureFolderBtn;
        [SerializeField] private Button m_perviewBtn;
        [SerializeField] private Button m_clearCreated;
        [SerializeField] private GameObject mainPanel;
        [SerializeField] private GameObject createdMark;
        [SerializeField] private HeadList headList;
        [SerializeField] private DetailList detailList;
        [SerializeField] private RectTransform previewContent;
        [SerializeField] private GameObject previewPanel;
        [SerializeField] private Button m_hidePreviewBtn;
        [SerializeField] private Button m_fullScreenBtn;
        [SerializeField] private Toggle m_toggleTree;
        [SerializeField] private FlashMask m_flashMask;
        [SerializeField] private Font defultFont;

        private Dictionary<string, Type> layerImportTypes;
        private List<string> controlTypes;
        private Dictionary<string, ILayerDefine> controlDic;
        private Type emptyImporter;
        private GameObject created;
        private UIInfo uiInfo;
        private LayerTree tree;
        private LayerTreeNodeGUI treeNodeGUI;
        private PreferString psdFilePath = new PreferString("UGUIAssembler.Config.psdFilePath","");
        private bool inPreviewAll
        {
            get
            {
                return m_toggleTree.isOn;
            }
            set
            {
                m_toggleTree.isOn = value;
            }
        }
        private Vector2 scrollPos;
        private void Awake()
        {
            m_import.onClick.AddListener(ImportConfigClick);
            m_renew.onClick.AddListener(ImportRenewConfigClick);
            m_export.onClick.AddListener(ExportConfigClick);
            m_psdImport.onClick.AddListener(ImportPSD);
            m_spriteFolder.text = PreferHelper.defultSpriteFolder;
            m_textureFolder.text = PreferHelper.textureFolderPath;

            m_spriteFolder.onValueChanged.AddListener(OnSpriteFolderChanged);
            m_textureFolder.onValueChanged.AddListener(OnTextureFolderChanged);

            m_spriteFolderBtn.onClick.AddListener(ChoiseSpriteFolder);
            m_textureFolderBtn.onClick.AddListener(ChoiseTextureFolder);
            m_perviewBtn.onClick.AddListener(OnPerviewClick);
            m_clearCreated.onClick.AddListener(OnClearClick);
            m_hidePreviewBtn.onClick.AddListener(OnHidePreview);
            m_fullScreenBtn.onClick.AddListener(ShowFullScreen);

            SwitchView(false);
            previewPanel.SetActive(false);
            m_flashMask.Hide();

            InitConfigEditor();

            detailList.onLayerNameChanged = OnItemNameChanged;
            detailList.onLayerDeleted = OnItemDeleted;
            detailList.onSelectChanged = OnDetailItemSelectChanged;
            detailList.onPreviewItem = OnPreviewItem;
            detailList.controlTypes = controlTypes;
            detailList.controlDic = controlDic;
            detailList.onInsert = OnInsetControlDetail;

            headList.onSelectChanged = OnHeadItemSelectChanged;
            headList.onPathChanged = OnPathChanged;
            headList.onInsert = OnInsetControl;
            headList.onSortChanged = OnHeadItemSortChanged;

            tree = new LayerTree();
            treeNodeGUI = new LayerTreeNodeGUI(tree.root);
            treeNodeGUI.onSelect = SetActiveTarget;
            PreferHelper.defultFont = defultFont;
        }

        private void Update()
        {
            if(Input.GetKeyDown(KeyCode.F5))
            {
                UpdateView();
            }
        }

        private void SetActiveTarget(LayerData target)
        {
            if(target == null || target.transform == null)
            {
                m_flashMask.Hide();
                return;
            }

            if(target.isExpanded)
            {
                m_flashMask.Show(target.transform as RectTransform);
            }
            else
            {
                m_flashMask.Hide();
            }
        }

        private void OnGUI()
        {
            if (inPreviewAll)
            {
                var boxRect = new Rect(0, 0, Screen.width * 0.3f, Screen.height);
                using (var scroll = new GUILayout.ScrollViewScope(scrollPos, GUILayout.Height(boxRect.height)))
                {
                    scrollPos = scroll.scrollPosition;
                    GUILayoutUtility.GetRect(boxRect.width, 10);
                    treeNodeGUI.DrawTreeLayout();
                    GUILayoutUtility.GetRect(boxRect.width, 10);
                }
            }
        }

        private void OnHeadItemSortChanged(List<int> idList)
        {
            if (detailList != null && idList != null)
            {
                detailList.Sort(idList);
            }
        }

        private void OnHidePreview()
        {
            previewPanel.gameObject.SetActive(false);
        }

        private void InitConfigEditor()
        {
            emptyImporter = typeof(EmptyLayerImport);
            layerImportTypes = new Dictionary<string, Type>();
            layerImportTypes.Add("Text", typeof(TextLayerImport));
            layerImportTypes.Add("Image", typeof(ImageLayerImport));
            layerImportTypes.Add("Button", typeof(ButtonLayerImport));
            layerImportTypes.Add("Toggle", typeof(ToggleLayerImport));
            layerImportTypes.Add("InputField", typeof(InputFieldLayerImport));
            layerImportTypes.Add("Slider", typeof(SliderLayerImport));
            layerImportTypes.Add("RectTransform", typeof(RectTranformLayerImport));
            controlTypes = new List<string>();
            controlTypes.AddRange(layerImportTypes.Keys);
            controlDic = new Dictionary<string, ILayerDefine>();
            controlDic.Add("Text", new TextLayerDefine());
            controlDic.Add("Image", new ImageLayerDefine());
            controlDic.Add("RawImage", new RawImageLayerDefine());
            controlDic.Add("Button", new ButtonLayerDefine());
            controlDic.Add("Toggle", new ToggleLayerDefine());
            controlDic.Add("InputField", new InputFieldLayerDefine());
            controlDic.Add("Slider", new SliderLayerDefine());
            controlDic.Add("RectTransform", new RectTransformLayerDefine());
        }
        private void OnClearClick()
        {
            if (created != null)
            {
                Destroy(created);
                SwitchView(false);
                inPreviewAll = false;
                m_flashMask.Hide();
            }
        }

        private void OnInsetControl(int index)
        {
            var options = controlDic.Keys.ToArray();
            PopOption.Instence.ShowPop(options, (id) =>
            {
                var type = options[id];
                var name = "new " + type;
                var path = name;
                var rect = new Rect();
                if (index >= 0)
                {
                    var layerInfo = detailList.GetLayerInfoFromIndex(index);
                    path = layerInfo.path + "/" + name;
                    rect = layerInfo.rect;
                }
                else
                {
                    if (uiInfo == null)
                        uiInfo = new UIInfo("new ui");
                    detailList.SetUIInfo(uiInfo);
                }

                headList.InsetItem(index + 1, name,path);
                detailList.InsetItem(index + 1, name, path, type, rect);
            });
        }

        private void OnInsetControlDetail(int index,LayerInfo layerInfo)
        {
            headList.InsetItem(index, layerInfo.name, layerInfo.path);
        }

        private void ShowFullScreen()
        {
            if (created != null)
            {
                created.transform.SetParent(targetCanvas.transform, false);
                (created.transform as RectTransform).anchoredPosition = Vector2.zero;
                SwitchView(true);
                previewPanel.SetActive(false);
                inPreviewAll = true;
                tree.SetTransform(created.transform);
            }
        }

        private void OnHeadItemSelectChanged(int index, bool active)
        {
            detailList.SetActiveItem(index, active);
        }

        private void OnPathChanged(int index,string path)
        {
            uiInfo.layers[index].path = path;
        }

        private void OnDetailItemSelectChanged(int index, bool active)
        {
            headList.SetActiveItem(index, active);
        }

        private void OnItemNameChanged(int index,string layerName)
        {
            headList.UpdateItem(index, layerName);
        }

        private void OnItemDeleted(int id)
        {
            headList.DeleteAtID(id);
        }

        private void OnPreviewItem(int index)
        {
            if (uiInfo.layers.Count > index && index >= 0)
            {
                var layerInfo = uiInfo.layers[index];
                var tempUIInfo = new UIInfo(layerInfo.name);
                tempUIInfo.layers.Add(layerInfo);
                if (created != null)
                    Destroy(created);
                Assembler.emptyImporter = emptyImporter;
                created = Assembler.GenerateUI(PreferHelper.defultSpriteFolder, targetCanvas, layerImportTypes, tempUIInfo);
                ShowToPreviewPanel();
            }
            else
            {
                DialogHelper.ShowDialog("无法预览", "无层级信息：" + index,false);
            }
        }

        private void ImportPSD()
        {
            var psdFile = DialogHelper.OpenPSDFileDialog("psd文件", psdFilePath.Value);
            if (!string.IsNullOrEmpty(psdFile))
            {
                psdFilePath.Value = psdFile;
                var fileName = System.IO.Path.GetFileNameWithoutExtension(psdFile);
                using (PsdDocument doc = PsdDocument.Create(psdFile))
                {
                    this.uiInfo = ExportUtility.CreatePictures(doc,fileName);
                    UpdateView();
                }
            }
        }

        private void OnPerviewClick()
        {
            if (uiInfo != null)
            {
                Assembler.emptyImporter = emptyImporter;

                if (created != null)
                    Destroy(created);
                created = Assembler.GenerateUI(PreferHelper.defultSpriteFolder, targetCanvas, layerImportTypes, uiInfo);
                ShowFullScreen();
                inPreviewAll = true;
                tree.SetTransform(created.transform);
            }
        }

        private void ShowToPreviewPanel()
        {
            if (created != null)
            {
                created.transform.SetParent(previewContent, false);
                (created.transform as RectTransform).anchoredPosition = Vector2.zero;
                previewPanel.SetActive(true);
            }
        }

        private void SwitchView(bool fullScreenUI)
        {
            mainPanel.SetActive(!fullScreenUI);
            createdMark.SetActive(fullScreenUI);
        }


        private void OnSpriteFolderChanged(string arg0)
        {
            PreferHelper.defultSpriteFolder = arg0;
        }

        private void OnTextureFolderChanged(string arg0)
        {
            PreferHelper.textureFolderPath = arg0;
        }

        private void ChoiseSpriteFolder()
        {
            var path = DialogHelper.OpenFolderDialog("选择路径", PreferHelper.defultSpriteFolder);
            if (!string.IsNullOrEmpty(path))
            {
                m_spriteFolder.text = path;
            }
        }

        private void ChoiseTextureFolder()
        {
            var path = DialogHelper.OpenFolderDialog("选择路径", PreferHelper.textureFolderPath);
            if (!string.IsNullOrEmpty(path))
            {
                m_textureFolder.text = path;
            }
        }


        private void ImportConfigClick()
        {
            this.uiInfo = LoadCsvAndCreateUIInfo();
            if(this.uiInfo != null)
            {
                UpdateView();
            }
        }

        /// <summary>
        /// 按关键字更新层级信息
        /// </summary>
        private void ImportRenewConfigClick()
        {
            var newuiInfo = LoadCsvAndCreateUIInfo();
            if(newuiInfo != null)
            {
                if(this.uiInfo == null)
                {
                    this.uiInfo = newuiInfo;
                }
                else
                {
                    RenewUIInfo(this.uiInfo, newuiInfo);
                }
                UpdateView();
            }
        }

        private void RenewUIInfo(UIInfo uiInfo,UIInfo newInfo)//私有方法，在本类中使用，在确定不会传递空参数时可以不用判断
        {
            var uiInfoDic = MakeLayerInfoDic(uiInfo);
            for (int i = 0; i < newInfo.layers.Count; i++)
            {
                var layer = newInfo.layers[i];
                if(uiInfoDic.ContainsKey(layer.name) && uiInfoDic[layer.name].Count > 0)
                {
                    var oringalLayer = uiInfoDic[layer.name].Dequeue();
                    UpdateLayerInfo(oringalLayer, layer);
                }
                else
                {
                    uiInfo.layers.Add(layer);
                }
            }
        }

        /// <summary>
        /// 更新层级信息
        /// </summary>
        /// <param name="layerInfo"></param>
        /// <param name="newLayerInfo"></param>
        private void UpdateLayerInfo(LayerInfo layerInfo,LayerInfo newLayerInfo)
        {
            layerInfo.path = newLayerInfo.path;
            layerInfo.rect = newLayerInfo.rect;
            UpdateResourceDic(layerInfo.resourceDic, newLayerInfo.resourceDic);
            UpdateSubResourceDic(layerInfo.subResourceDic, newLayerInfo.subResourceDic);
            UpdateDicArray("new Image",layerInfo.sub_images, newLayerInfo.sub_images);
            UpdateDicArray("new Text", layerInfo.sub_texts, newLayerInfo.sub_texts);
            UpdateDicArray("new RawImage", layerInfo.sub_rawImages, newLayerInfo.sub_rawImages);
        }

        private void UpdateDicArray(string defultName,List<ResourceDic> dicArray,List<ResourceDic> newDicArray)
        {
            if (newDicArray.Count == 0) return;
            var dic = MakeDicArryDic(defultName, dicArray);
            using (var enumerator = newDicArray.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    var current = enumerator.Current;
                    var dicName = defultName;
                    if(current.ContainsKey("name"))
                    {
                        dicName = current["name"];
                    }

                    if(dic.ContainsKey(dicName) && dic[dicName].Count > 0)
                    {
                        var infoDic = dic[dicName].Dequeue();
                        UpdateResourceDic(infoDic, current);
                    }
                    else
                    {
                        dicArray.Add(current);
                    }
                }
            }
        }
        /// <summary>
        /// 更新子控件字典
        /// </summary>
        /// <param name="subDic"></param>
        /// <param name="newSubDic"></param>
        private void UpdateSubResourceDic(Dictionary<string, ResourceDic> subDic,Dictionary<string, ResourceDic> newSubDic)
        {
            if (newSubDic.Count == 0) return;

            using (var enumerator = newSubDic.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    var current = enumerator.Current;
                    if(subDic.ContainsKey(current.Key))
                    {
                        UpdateResourceDic(subDic[current.Key], current.Value);
                    }
                    else
                    {
                        subDic[current.Key] = new ResourceDic(current.Value);
                    }
                }
            }
        }

        /// <summary>
        /// 更新资源字典
        /// </summary>
        /// <param name="resourceDic"></param>
        /// <param name="newResourceDic"></param>
        private void UpdateResourceDic(ResourceDic resourceDic,ResourceDic newResourceDic)
        {
            if (newResourceDic.Count == 0) return;

            using (var enumerator = newResourceDic.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    var current = enumerator.Current;
                    resourceDic[current.Key] = current.Value;
                }
            }
        }

        /// <summary>
        /// 建立字典
        /// </summary>
        /// <param name="uiInfo"></param>
        /// <returns></returns>
        private Dictionary<string, Queue<LayerInfo>> MakeLayerInfoDic(UIInfo uiInfo)
        {
            var dic = new Dictionary<string, Queue<LayerInfo>>();

            for (int i = 0; i < uiInfo.layers.Count; i++)
            {
                var layer = uiInfo.layers[i];
                if (string.IsNullOrEmpty(layer.name))
                {
                    Debug.LogError("层级名称为空,无法正常更新：" + layer.path);
                    continue;
                }
                if(!dic.ContainsKey(layer.name))
                {
                    dic.Add(layer.name, new Queue<LayerInfo>());
                }
                dic[layer.name].Enqueue(layer);
                if(dic[layer.name].Count > 1){
                    Debug.LogError("层级名称重复,无法正常更新：" + layer.name + ":" + layer.path);
                }
            }
            return dic;
        }

        /// <summary>
        /// 按名称建立字典
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        private Dictionary<string,Queue<ResourceDic>> MakeDicArryDic(string defultName,List<ResourceDic> list)
        {
            var dic = new Dictionary<string, Queue<ResourceDic>>();
            using (var enumerator = list.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    var current = enumerator.Current;
                    if(!current.ContainsKey("name"))
                    {
                        current.Add("name", defultName);
                    }
                    var dicName = current["name"];
                    if(!dic.ContainsKey(dicName))
                    {
                        dic.Add(dicName, new Queue<ResourceDic>());
                    }
                    dic[dicName].Enqueue(current);
                }
            }
            return dic;
        }

        private UIInfo LoadCsvAndCreateUIInfo()
        {
            var spriteFolderPath = PreferHelper.defultSpriteFolder;
            if (string.IsNullOrEmpty(spriteFolderPath))
            {
                DialogHelper.ShowDialog("错误提示", "图片加载路径不能为空", "确认");
            }
            else
            {
                var configPath = GetConfigFilePath();
                CsvTable table = null;
            pos_readdoc:
                if (File.Exists(configPath))
                {
                    try
                    {
                        table = CsvHelper.ReadCSV(configPath, System.Text.Encoding.GetEncoding("GB2312"));
                    }
                    catch (Exception e)
                    {
                        var reopen = DialogHelper.ShowDialog("提示", e.Message, "重试", "取消");
                        if (reopen)
                        {
                            goto pos_readdoc;
                        }
                    }

                    if (table != null)
                    {
                        var canLoad = table.IsUIInfoTable(false);
                        if (canLoad)
                        {
                            var isTitleMatch = table.IsUIInfoTable(true);
                            if (!isTitleMatch)
                            {
                                var forceLoad = DialogHelper.ShowDialog("文档标题不匹配", string.Join(",", UIInfo_TableExtend.uiInfoHead) + "\n继续请按确认！", "确认", "取消");
                                if (!forceLoad)
                                {
                                    return null;
                                }
                            }

                            return table.LoadUIInfo();
                        }
                        else
                        {
                            DialogHelper.ShowDialog("配制文档不可用", "请核对后重试！", "确认");
                        }
                    }
                }
            }
            return null;
        }

        private void ExportConfigClick()
        {
            if (uiInfo == null) return;

            var configPath = SaveConfigFilePath();
            if (!string.IsNullOrEmpty(configPath))
            {
                var table = uiInfo.UIInfoToTable();
            trySave: try
                {
                    CsvHelper.SaveCSV(table, configPath, System.Text.Encoding.GetEncoding("gb2312"));
                    DialogHelper.OpenFolderAndSelectFile(configPath);
                }
                catch (Exception e)
                {
                    var retry = DialogHelper.ShowDialog("保存失败", e.Message + "\n重试请按确认！", true);
                    if (retry)
                    {
                        goto trySave;
                    }
                }
            }
        }

        private void UpdateView()
        {
            if (uiInfo != null)
            {
                headList.SetInfo(uiInfo.layers.Select(x =>new string[] { x.name, x.path }).ToList());
                detailList.SetUIInfo( uiInfo);
            }
        }

        private string GetConfigFilePath()
        {
            string configFile = DialogHelper.OpenCSVFileDialog("选择配制文档", PreferHelper.configFolderPath);

            if (!string.IsNullOrEmpty(configFile))
            {
                PreferHelper.configFolderPath = System.IO.Path.GetDirectoryName(configFile);
            }
            return configFile;
        }

        private string SaveConfigFilePath()
        {
            if (uiInfo == null) return null;

            string configFile = DialogHelper.SaveCsvFileDialog("保存配制文档", uiInfo.name, PreferHelper.configFolderPath);
            return configFile;
        }

    }
}