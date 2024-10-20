🌌 XTC-00 Url Fuzzer is a Simple Program written in C# made to find hidden directories/files on a website.🌌


![Screenshot 2024-10-20 223711](https://github.com/user-attachments/assets/59107699-ef2d-4d98-a1db-b57952961ae6)

A default wordlist containing more than 30k interesting paths has been included.
Checking speed is estimated 3-4k paths per minute.

results get organized by HTTP status codes to make analysis easier. For each tested URL, it records the server's response status, like 200 for success or 404 for not found. 
It then saves the details in a file named after the status code (e.g., 200.txt, 404.txt). This way, all URLs with the same response type are grouped together, making it simple to review which URLs succeeded or failed.

This Program is Unobfuscated therefore you can extract the source code using dnspy (or download it from here) and edit it to your liking.

Here is A Video Preview of the Fuzzer:

https://www.youtube.com/watch?v=eZkBxMRIrR8
