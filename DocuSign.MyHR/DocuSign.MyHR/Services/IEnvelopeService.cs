﻿using DocuSign.MyHR.Domain;

namespace DocuSign.MyHR.Services
{
    public interface IEnvelopeService
    {
        string CreateEnvelope(DocumentType type, string accountId, string userId, string redirectUrl, string pingAction);
    }
}