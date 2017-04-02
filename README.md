# RedditPlaceBot.NET
Multi-account image drawing bot for reddit.com/place

## Build Instructions
1. Restore Nuget packages
```
EmguCV
Newtonsoft.JSON
```

2. Build with `msbuild` or Visual Studio on Windows or `xbuild` on Linux
```
xbuild /p:Configuration=Release RPlaceBot.sln
```
Note: For Linux builds, you must obtain the opencv binaries for your respective distribution

3. In the assembly directory (`bin/Release`), create `RedditAccounts.xml` with format:
```xml
<RedditAccounts>
  <Account>
    <Username>{REDDIT_USERNAME}</Username>
    <Password>{REDDIT_PASSWORD}</Password>
  </Account>
  <Account>
    <Username>{REDDIT_USERNAME}</Username>
    <Password>{REDDIT_PASSWORD}</Password>
  </Account>
  ...
</RedditAccounts>
```

4. Run the executable.

## Usage
1. Enter relative path to image to draw when prompted, for example, `image.png`
2. Enter pixel coordinates in the form `x y`, for example, `10 20`<br>
   OR, enter `optimize` to find the location on the map that would take the least time to finish the image.
3. The bot will now progress, time estimates and the image so far will be printed every 5 minutes or so.
