# cs-fountain-codes: <i>a few rateless forward error-correcting codes</i>
Featuring:
* LT Codes
* Random subset
* ...and others

Fountain codes take a message of ![k](http://www.sciweavers.org/tex2img.php?eq=k&bc=White&fc=Black&im=png&fs=12&ff=txfonts&edit=0) symbols and can generate a virtually unlimited number of encoding symbols. Any subset of ![n](http://www.sciweavers.org/tex2img.php?eq=n&bc=White&fc=Black&im=png&fs=12&ff=txfonts&edit=0) encoding symbols suffices for decoding the message. Better fountain codes allow for smaller ![n](http://www.sciweavers.org/tex2img.php?eq=n&bc=White&fc=Black&im=png&fs=12&ff=txfonts&edit=0)'s

## LT Codes
The LT Code implementation is based on Michael Luby's <u>LT Codes</u> paper (http://ieeexplore.ieee.org/stamp/stamp.jsp?tp=&arnumber=1181950&isnumber=26517). Basically the number of chunks that get combined together into each encoding symbol comes from the Robust Soliton Distribution.
### ![n](http://www.sciweavers.org/tex2img.php?eq=n&bc=White&fc=Black&im=png&fs=24&ff=txfonts&edit=0) vs ![k](http://www.sciweavers.org/tex2img.php?eq=k&bc=White&fc=Black&im=png&fs=24&ff=txfonts&edit=0)
![n vs k colored by p](https://raw.githubusercontent.com/matthew-a-thomas/cs-fountain-codes/master/n%20vs%20k%20(colored%20by%20p%3B%20with%20jitter).png "n vs k colored by p")
Note the color indicates ![p](http://www.sciweavers.org/tex2img.php?eq=p&bc=White&fc=Black&im=png&fs=12&ff=txfonts&edit=0), the probability of packet erasure

## Notices
* JavaReedSolomon was obtained from (https://github.com/Backblaze/JavaReedSolomon). They use the MIT license (https://github.com/Backblaze/JavaReedSolomon/blob/master/LICENSE)
* OpenRQ was obtained from (https://github.com/openrq-team/OpenRQ). They use the Apache 2.0 license (https://github.com/openrq-team/OpenRQ/blob/master/LICENSE)
* JavaReedSolomon and OpenRQ were compiled into .Net .DLLs using IKVMC. They use their own license (http://www.ikvm.net/license.html)
