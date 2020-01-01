# Procedural Trees
LSystem demonstration for Unity.
Based on the book [The Algorithmic Beauty of Plants](https://www.amazon.com/Algorithmic-Beauty-Plants-Virtual-Laboratory/dp/0387946764)

![Looping L-system](https://media.giphy.com/media/JNmA943xA83kQpToPm/giphy.gif)

The above example has a fully deterministic "random" loop. 

![Reproducing results in the book](https://media.giphy.com/media/St3rRONeAxaap0PEVK/giphy.gif)

Accurately reproduces patterns shown in the book.


![Seed](https://media.giphy.com/media/Q7Wf3iIPOP8y4xIRV5/giphy.gif)

A naive implementation using text. Quite slow but easy to understand. It allows you to map characters to arbitrary behavior by extending `LSystem.Module` It's pretty extensible but has some performance issues.

![Seed](https://media.giphy.com/media/f8PbJ8sqfWptR9b1Sq/giphy.gif)

There is also a procedural growing leaf script that generates a mesh on the fly and connects to the LSystem.

# Notes
- This was a demonstration made for a job interview, It can be slow. **Depending on your use case I would be wary to use this in production without some optimization**  There are some tricks already being used to improve performance such as pre-baking but unfortunately, it's still not necessarily realtime depending on your computer.
- The parameters have been modified a bit since these gifs were taken but the underlying algorithm works the same.


