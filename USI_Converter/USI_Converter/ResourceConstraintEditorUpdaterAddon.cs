using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace USI
{
    [KSPAddon(KSPAddon.Startup.EditorAny, false)]
    public class ResourceConstraintEditorUpdaterAddon : MonoBehaviour
    {
        private const string ModuleName = "USI_Converter";
        private static short _updateCounter;
        private static List<USI_ConverterModuleWrapper> _moduleList;
        private static object _moduleListLock;

        public void Update()
        {
            if (_updateCounter > 0)
            {
                _updateCounter--;
                return;
            }
            _updateCounter = 15;
            ProcessModuleList();
        }

        public void Awake()
        {
            _updateCounter = 0;
            _moduleList = new List<USI_ConverterModuleWrapper>();
            _moduleListLock = new object();
            GameEvents.onPartAttach.Add(HandlePartAttach);
            GameEvents.onPartRemove.Add(HandlePartRemove);
        }

        public void OnDestroy()
        {
            GameEvents.onPartAttach.Remove(HandlePartAttach);
            GameEvents.onPartRemove.Remove(HandlePartRemove);
        }

        private static void ProcessModuleList()
        {
            lock (_moduleListLock)
            {
                var delList = new List<USI_ConverterModuleWrapper>();
                foreach (var moduleWrapper in _moduleList)
                {
                    if (moduleWrapper.Module == null)
                    {
                        delList.Add(moduleWrapper);
                    }
                    else
                    {
                        moduleWrapper.Module.CollectResourceConstraintData();
                    }
                }
                foreach (var moduleWrapper in delList)
                {
                    _moduleList.Remove(moduleWrapper);
                }
            }
        }

        private void HandlePartRemove(GameEvents.HostTargetAction<Part, Part> data)
        {
            var parts = GetPartList(data.target);
            var modules = parts.Where(p => p.Modules.Contains(ModuleName)).Select(p => p.Modules[ModuleName] as USI_Converter).ToList();
            lock (_moduleListLock)
            {
                foreach (var module in modules.Where(ListContainsModule))
                {
                    foreach (var moduleWrapper in _moduleList)
                    {
                        if (moduleWrapper.Module != null && moduleWrapper.Module.ID == module.ID)
                        {
                            _moduleList.Remove(moduleWrapper);
                            break;
                        }
                    }
                }
            }
        }

        private void HandlePartAttach(GameEvents.HostTargetAction<Part, Part> data)
        {
            var parts = GetPartList(data.host);
            var modules = parts.Where(p => p.Modules.Contains(ModuleName)).Select(p => p.Modules[ModuleName] as USI_Converter).ToList();
            foreach (var module in modules)
            {
                module.ID = Guid.NewGuid();
            }
            lock (_moduleListLock)
            {
                foreach (var module in modules.Where(module => !ListContainsModule(module)))
                {
                    _moduleList.Add(new USI_ConverterModuleWrapper(module));
                }
            }
        }

        private static bool ListContainsModule(USI_Converter module)
        {
            return _moduleList.Any(wrapper => wrapper.Module != null && wrapper.Module.ID == module.ID);
        }

        private static IEnumerable<Part> GetPartList(Part root)
        {
            var list = new List<Part>();
            RecursePartList(list, root);
            return list;
        }

        private static void RecursePartList(ICollection<Part> list, Part part)
        {
            list.Add(part);
            foreach (var p in part.children)
            {
                RecursePartList(list, p);
            }
        }

        private struct USI_ConverterModuleWrapper
        {
            internal USI_Converter Module { get; private set; }

            internal USI_ConverterModuleWrapper(USI_Converter module)
                : this()
            {
                Module = module;
            }
        }
    }
}