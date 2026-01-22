using System;
using System.Collections.Generic;

[Serializable]
public class VocabTerm
{
    public string word;
    [UnityEngine.TextArea] public string definition;
}

[Serializable]
public class VocabList
{
    public List<VocabTerm> terms = new List<VocabTerm>();
}