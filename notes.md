# Dev notes

`dotnet watch` to start an app with the hot reload

Right now I'm trying to implement a weird "streaming" way of sending images to the client.
By streaming I mean that instead of sending image files one by one with an `image/jpeg` MIME type
I want to have a long living Response with a byte stream. So all images go there - plus metadata to be
able to say when one image ends.
Right now I've implemented some simple writer on a Backend, but I was unable to handle it on a front.
It does work with a CURL request - it logs that response comes in chunks.
Also it's fine in FF
But Chrome waits for Response to end.
However it seems that the Streams API is the right way to handle it on the clent.
For the future reference see https://developer.mozilla.org/en-US/docs/Web/API/ReadableStream/getReader