# cs-fountain-codes: <i>a few rateless forward error-correcting codes</i>
Featuring:
* [LT Codes](#lt-code)
* ["Special" LT Code](#special-lt-code)
* [Random subset](#random-subset)
* [Sophisticated carousel](#sophisticated-carousel)
* [Plain old carousel](#plain-old-carousel)

Fountain codes turn a message of *k* symbols into encoding symbols such that any subset of *n*≥*k* encoding symbols will decode the message.

This is great when you need to pass data through a lossy channel: as long as you can get *n* encoding symbols across the wire, the recipient will get the message.

Better fountain codes allow for smaller *n*'s (and some get *n* very close to *k*, meaning very little overhead).

The [Reed Solomon](https://en.wikipedia.org/wiki/Reed%E2%80%93Solomon_error_correction) error-correcting code, on the other hand, cannot produce additional check symbols once it has been constructed.

## Implementations
### LT Code
The LT Code implementation is based on Michael Luby's <u>LT Codes</u> paper (http://ieeexplore.ieee.org/stamp/stamp.jsp?tp=&arnumber=1181950&isnumber=26517). Basically the number of chunks that get combined together into each encoding symbol comes from the Robust Soliton Distribution.
#### <i>n</i> vs <i>k</i>
![n vs k colored by p](https://github.com/matthew-a-thomas/cs-fountain-codes/raw/master/lt%20code%20-%20n%20vs%20k%20(colored%20by%20p).png "n vs k colored by p")
Note the color indicates <i>p</i>, the probability of packet erasure. Orange is <i>p=1</i>; blue is <i>p=0</i>

### "Special" LT Code
The "special" LT code represents the limit of the LT code as the delta parameter of the Robust Soliton Distribution approaches zero. In other words, a constant portion of message parts are combined together for each encoding symbol.
#### <i>n</i> vs <i>k</i>
![n vs k colored by p](https://github.com/matthew-a-thomas/cs-fountain-codes/raw/master/special%20lt%20-%20n%20vs%20k%20(colored%20by%20p).png "n vs k colored by p")
Note the color indicates <i>p</i>, the probability of packet erasure. Orange is <i>p=1</i>; blue is <i>p=0</i>

### Random subset
The random subset fountain code picks uniformly randomly from the message. Each part of the message has equal probability of being included in the combination to make an encoding symbol. Encoding symbols will always include at least one part, though.
#### <i>n</i> vs <i>k</i>
![n vs k colored by p](https://github.com/matthew-a-thomas/cs-fountain-codes/raw/master/random%20subset%20-%20n%20vs%20k%20(colored%20by%20p).png "n vs k colored by p")
Note the color indicates </i>p</i>, the probability of packet erasure. Orange is <i>p=1</i>; blue is <i>p=0</i>

### Sophisticated carousel
The sophisticated carousel fountain code is a glorified round-robin repeat. Instead of just cycling through sequantial parts of the message and sending those parts solo, this fountain code cycles through the multiplicative group (the non-zero elements) of a Galois Field. The bits that are set in the decimal form of the finite field element determine which parts get combined to make an encoding symbol. Finite fields have a neat property where any <i>k</i> consecutive encoding symbols are enough to decode the message.
#### <i>n</i> vs <i>k</i>
![n vs k colored by p](https://github.com/matthew-a-thomas/cs-fountain-codes/raw/master/sophisticated%20carousel%20-%20n%20vs%20k%20(colored%20by%20p%3B%20with%20jitter).png "n vs k colored by p")
Note the color indicates <i>p</i>, the probability of packet erasure. Orange is <i>p=1</i>; blue is <i>p=0</i>

Notice the inversion of <i>p</i>. Higher probabilities of packet erasure yield lower values of <i>n</i>. So do the lowest, but only the very lowest values of <i>p</i> so it's difficult to see. This means this fountain code performs best in either the most clean communications channel, or in the worst. Mathematically, this fountain code running through the worst channel (highest <i>p</i>) is equivalent to the random subset fountain code above.

Also notice the different "modes" that this fountain code appears to take on: one mode has significantly higher variances than the other and seems to depend strongly on particular values of <i>k</i>. This is probably due to some mathematical relationship I haven't learned about yet. Likely it's related to which primitive polynomial for the Galois Field is used for each value of <i>k</i>.

### Plain old carousel
The regular carousel probably isn't properly a fountain code. It just cycles through individual parts of the message, sending them one at a time solo. If the receiver misses a symbol it has to wait for the sender to cycle all the way back through the <i>k</i> message symbols.
#### <i>n</i> vs <i>k</i>
![n vs k colored by p](https://github.com/matthew-a-thomas/cs-fountain-codes/raw/master/carousel%20-%20n%20vs%20k%20(colored%20by%20p%3B%20with%20jitter).png "n vs k colored by p")
Note the color indicates <i>p</i>, the probability of packet erasure. Orange is <i>p=1</i>; blue is <i>p=0</i>

## Notices
* JavaReedSolomon was obtained from (https://github.com/Backblaze/JavaReedSolomon). They use the MIT license (https://github.com/Backblaze/JavaReedSolomon/blob/master/LICENSE)
* OpenRQ was obtained from (https://github.com/openrq-team/OpenRQ). They use the Apache 2.0 license (https://github.com/openrq-team/OpenRQ/blob/master/LICENSE)
* JavaReedSolomon and OpenRQ were compiled into .Net .DLLs using IKVMC. They use their own license (http://www.ikvm.net/license.html)
* Some images were generated using Weka (http://www.cs.waikato.ac.nz/ml/weka/)
