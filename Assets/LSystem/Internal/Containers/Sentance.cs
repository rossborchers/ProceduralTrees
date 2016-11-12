
/// <summary>
/// Encapsulates string sentence and provides a current index
/// </summary>
public class Sentence
{
    protected int position;
    protected string sentence;
    
    public Sentence(Sentence old)
    {
        position = old.position;
        sentence = old.sentence;
    }

    public Sentence(string sentence)
    {
        this.sentence = sentence;
        this.position = 0;
    }

    public Sentence(string sentence, int position)
    {
        this.sentence = sentence;
        this.position = position;
    }

    public Sentence Set(string sentanceStr)
    {
        sentence = sentanceStr;
        return this;
    }

    public Sentence Append(string sentanceStr)
    {
        sentence += sentanceStr;
        return this;
    }

    public Sentence Prepend(string sentanceStr)
    {
        sentence = sentanceStr + sentence;
        return this;
    }

    public Sentence Set(Sentence sentance)
    {
        this.sentence = sentance.sentence;
        return this;
    }

    public Sentence Append(Sentence sentance)
    {
        this.sentence += sentance.sentence;
        return this;
    }

    public Sentence Prepend(Sentence sentance)
    {
        this.sentence = sentance.sentence + this.sentence;
        return this;
    }

    public Sentence Remove(int start, int count)
    {
        this.sentence.Remove(start, count);
        return this;
    }

    public bool HasNext()
    {
        return (position < sentence.Length);
    }

    public int Position()
    {
        return position;
    }

    public char Next()
    {
        char next = sentence.ToCharArray()[position];
        if (HasNext())
        {
            position++;
        }
        return next;
    }

  

    public char PeekNext()
    {
        char next = (char)0;
        if (position + 1 < sentence.Length)
        {
            next = sentence.ToCharArray()[position + 1];
        }
        return next;
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
        }
    }

    public char[] ToCharArray()
    {
        return sentence.ToCharArray();
    }

}
