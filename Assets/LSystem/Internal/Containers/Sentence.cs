using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Encapsulates string sentence and provides a current index
/// </summary>
public class Sentence
{
    Stack<int> positionStack = new Stack<int>();

    protected int position;
    protected string sentence;

    public int UniqueCharacters { get { return uniqueCharacters; } private set { uniqueCharacters = value; } }
    protected int uniqueCharacters;
    
    public Sentence(Sentence old)
    {
        position = old.position;
        sentence = old.sentence;
        uniqueCharacters = old.uniqueCharacters;
    }

    public Sentence(string sentence)
    {
        this.sentence = sentence;
        this.position = -1;
        UpdateUniqueCharacters();
    }

    public Sentence(string sentence, int position)
    {
        this.sentence = sentence;
        this.position = position;
        UpdateUniqueCharacters();
    }

    public Sentence Set(string sentanceStr)
    {
        sentence = sentanceStr;
        UpdateUniqueCharacters();
        return this;
    }

    public Sentence Append(string sentanceStr)
    {
        sentence += sentanceStr;
        UpdateUniqueCharacters();
        return this;
    }

    public Sentence Prepend(string sentanceStr)
    {
        sentence = sentanceStr + sentence;
        UpdateUniqueCharacters();
        return this;
    }

    public Sentence Set(Sentence sentance)
    {
        this.sentence = sentance.sentence;
        UpdateUniqueCharacters();
        return this;
    }

    public Sentence Append(Sentence sentance)
    {
        this.sentence += sentance.sentence;
        UpdateUniqueCharacters();
        return this;
    }

    public Sentence Prepend(Sentence sentance)
    {
        this.sentence = sentance.sentence + this.sentence;
        UpdateUniqueCharacters();
        return this;
    }

    public Sentence Remove(int start, int count)
    {
        this.sentence.Remove(start, count);
        UpdateUniqueCharacters();
        return this;
    }

    protected void UpdateUniqueCharacters()
    {
        HashSet<char> unique = new HashSet<char>();
        foreach(char c in sentence)
        {
            unique.Add(c);
        }
        uniqueCharacters = unique.Count;
    }

    public bool HasNext()
    {
        return (position + 1 < sentence.Length);
    }

    public int Position()
    {
        return position;
    }

    public char Next()
    {
        if (HasNext())
        {
            position++;
        }
        else if (position < 0) return '\0';
        return sentence.ToCharArray()[position];
    }

    public char Current()
    {
        if (position < 0) return '\0';
        return sentence.ToCharArray()[position];
    }

    public override string ToString()
    {
        return sentence;
    }

    public void PushPosition()
    {
        positionStack.Push(position);
    }

    public void PopPosition()
    {
       position = positionStack.Pop();
    }

    /// <summary>
    /// Removes inclusive of pop and push positions. Returns the removed <see cref="Sentence"/> minus pop and push chars.
    /// </summary>
    /// <returns> A <see cref="Sentence"/> exclusive of the chars at the pop and push positions.</returns>
    public Sentence PopPositionAndCut()
    {
        int pushState = position;
        PopPosition();
        Sentence cut = new Sentence(sentence.Substring(position + 1, pushState - position -1));

        //position is now on new char, or out of bounds if pop was on last char
        sentence = sentence.Remove(position, pushState - position + 1);

        position--;

        return cut;
    }

    public char this[int index]
    {
        get
        {
            return sentence.ToCharArray()[index];
        }
        set
        {
            char[] sentenceChars = sentence.ToCharArray();
            sentenceChars[index] = value;
            sentence = new string(sentenceChars);
            UpdateUniqueCharacters();
        }
    }

    public char[] ToCharArray()
    {
        return sentence.ToCharArray();
    }

}
