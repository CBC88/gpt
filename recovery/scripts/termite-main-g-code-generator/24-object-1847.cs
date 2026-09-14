List<string> result = new List<string>(X.Count);

    foreach (double x in X)
    {
        result.Add(x.ToString("0.################",
            CultureInfo.InvariantCulture));
    }

    A = result;