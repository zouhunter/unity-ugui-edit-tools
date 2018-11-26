using System;
using System.Collections.Generic;
using System.Linq;

namespace UGUIAssembler
{
    public class YAMLite
    {
        public class Node : Dictionary<string, Node>
        {
            public string leaf = null;
            private bool initialized = false;
            public Node parent;

            public static implicit operator bool(Node n)
            {
                return n != null && n.Count != 0 || n.leaf != null;
            }

            public static implicit operator string(Node n)
            {
                if (n.leaf != null)
                {
                    return n.leaf;
                }
                else if (n.Count == 0)
                {
                    //Null node
                    return "~";
                }
                else
                {
                    //Object node
                    string yaml = "";
                    foreach (KeyValuePair<string, Node> entry in n)
                    {
                        yaml += entry.Key + ":";
                        if (entry.Value.leaf != null)
                        {
                            yaml += " " + entry.Value + "\n";
                        }
                        else
                        {
                            foreach (string line in entry.Value.ToString().Split(new char[] { '\n' }, System.StringSplitOptions.RemoveEmptyEntries).ToList())
                            {
                                yaml += "\n  " + line;
                            }
                            yaml += "\n";
                        }
                    }
                    return yaml;
                }
            }

            public static implicit operator Node(string s)
            {
                Node n = new Node();
                n.leaf = s;
                return n;
            }

            public Node merge(Node n)
            {

                if (this.leaf != null)
                {
                    return n;
                }

                foreach (KeyValuePair<string, Node> entry in n)
                {
                    if (!this.ContainsKey(entry.Key))
                    {
                        this.Add(entry.Key, entry.Value);
                    }
                    else
                    {
                        //Leaf node on either
                        if (this[entry.Key].leaf != null || entry.Value.leaf != null)
                        {
                            this[entry.Key] = entry.Value;

                            //Object node - recurse!
                        }
                        else
                        {
                            this[entry.Key].merge(entry.Value);
                        }
                    }
                }
                return this;
            }

            public new string ToString()
            {
                return (string)this;
            }

            public Node this[int key]
            {
                get
                {
                    return this[key.ToString()];
                }
                set
                {
                    this[key.ToString()] = value;
                }
            }

            public new Node this[string key]
            {
                get
                {
                    Node outNode = new Node();
                    TryGetValue(key, out outNode);
                    if (outNode == null)
                    {
                        return new Node();
                    }
                    else
                    {
                        return outNode;
                    }
                }
                set
                {
                    initialized = true;
                    this.Remove(key);
                    this.Add(key, value);
                }
            }
        }

        public static Node parse(string yaml)
        {
            yaml = yaml.TrimStart('\n');
            if (yaml.StartsWith("---")) { yaml = yaml.Remove(0, 3); }
            yaml = yaml.TrimStart('\n');
            return _parse(ref yaml, 0);
        }

        public static Node _parse(ref string yaml, int indentLevel)
        {

            //Detect unindented array
            bool unIndentedArray = false;
            if (yaml[0] == '\n' && yaml.Remove(0, 1).TrimStart(' ')[0] == '-')
            {
                int _indentLevel = yaml.Remove(0, 1).Length - yaml.Remove(0, 1).TrimStart(' ').Length;
                if (_indentLevel + 2 == indentLevel)
                {
                    yaml = yaml.Remove(0, 1);
                    unIndentedArray = true;
                }
            }

            int arrayCount = 0;
            Node n = new Node();
            while (yaml.Length != 0)
            {
                char c = yaml[0];

                switch (c)
                {
                    case ' ':
                        yaml = yaml.TrimStart(' ');
                        break;
                    case '\n':
                        yaml = yaml.Remove(0, 1);
                        int _indentLevel = yaml.Length - yaml.TrimStart(' ').Length;
                        if (unIndentedArray) { _indentLevel += 2; }
                        if (_indentLevel != indentLevel)
                        {
                            yaml = "\n" + yaml;
                            return n;
                        }
                        break;
                    case '-':
                        yaml = arrayCount++.ToString() + ": " + yaml.Remove(0, 1);
                        break;
                    default:
                        string sym = String.Join("", yaml.TakeWhile((_c) => _c != ':' && _c != '\n').Select(x=>x.ToString()).ToArray());
                        yaml = yaml.Remove(0, sym.Length);

                        if (yaml.Length > 0 && yaml[0] == ':')
                        {
                            yaml = yaml.Remove(0, 1).TrimStart(' ');

                            n.Add(sym, _parse(ref yaml, indentLevel + (unIndentedArray ? 0 : 2)));
                        }
                        else
                        {
                            n.leaf = sym;
                            return n;
                        }
                        break;
                }
            }

            return n;
        }
    }
}