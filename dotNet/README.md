MinusEngine
===========
A fully asynchronous C# library to access [Minus API](http://min.us/pages/api).

License
-------
The license for this project is [Apache Software License 2.0](http://www.apache.org/licenses/LICENSE-2.0.html).

It means you can pretty much do whatever you want with it IF you give back some credit.

Event handling
--------------
Every operation has two possible outputs: completion or failure.

Thus, for each operation on the class MinusApi (which in turn represents a call to the actual Minus site) you have to register a success and failure delegate to handle these events.

The asynchronous nature of the library makes it perfect to develop UI-based applications on top of it.

Code sample
-----------
    // create the API
    MinusApi api = new MinusApi("someDummyKey"); // api keys aren't working yet

    // setup the success handler for GetItems operation
    api.GetItemsComplete += delegate(MinusApi sender, GetItemsResult result)
    {
        Console.WriteLine("Gallery items successfully retrieved!\n---");
        Console.WriteLine("Read-only URL: " + result.ReadonlyUrl);
        Console.WriteLine("Title: " + result.Title);
        Console.WriteLine("Items:");
        foreach (String item in result.Items)
        {
            Console.WriteLine(" - " + item);
        }
    };

    // setup the failure handler for the GetItems operation
    api.GetItemsFailed += delegate(MinusApi sender, Exception e)
    {
        // don't do anything else...
        Console.WriteLine("Failed to get items from gallery...\n" + e.Message);
    };

    // trigger the GetItems operation - notice the extra "m" in there.
    // while the REAL reader id is "vgkRZC", the API requires you to put the extra "m" in there
    api.GetItems("mvgkRZC");

Contact
-------
If you have any questions, please drop me a line on twitter [@brunodecarvalho](http://twitter.com/brunodecarvalho) or email me at bruno@biasedbit.com

Dependencies
------------
This project uses the [Json.NET](http://json.codeplex.com/) library, by James Newton-King.