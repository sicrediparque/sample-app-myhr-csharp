﻿using System.Net.Http;

namespace DocuSign.MyHR.Services
{
    public interface IClickWrapService
    {
        HttpResponseMessage CreateTimeTrackClickWrap(string accountId, int[] workingLog);
    }
}