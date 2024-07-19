﻿using System.Formats.Cbor;

namespace KristofferStrube.Blazor.WebAuthentication;

public class PackedAttestationFormat
{
    /// <summary>
    /// The algorithm used to generate the attestation signature.
    /// </summary>
    public required COSEAlgorithm Algorithm { get; set; }

    /// <summary>
    /// A byte string containing the attestation signature.
    /// </summary>
    public required byte[] Signature { get; set; }

    /// <summary>
    /// The elements of this array contain attestation certificate and its certificate chain (if any), each encoded in X.509 format.
    /// The attestation certificate will be the first element in the array.
    /// </summary>
    public byte[][]? X5c { get; set; }

    public static PackedAttestationFormat ReadFromBase64EncodedAttestationStatement(string input)
    {
        CborReader cborReader = new(Convert.FromBase64String(input));

        CborReaderState state = cborReader.PeekState();

        if (state is not CborReaderState.StartMap)
        {
            throw new FormatException("Attestation Statement did not start with a map.");
        }

        int? mapSize = cborReader.ReadStartMap();
        if (mapSize is not 3)
        {
            throw new FormatException($"Attestation Statement had '{mapSize}' entries in its first map but '3' was expected.");
        }

        state = cborReader.PeekState();
        if (state is not CborReaderState.TextString)
        {
            throw new FormatException($"Attestation Statement's first key was of type '{state}' but '{CborReaderState.TextString}' was expected.");
        }

        string label = cborReader.ReadTextString();
        if (label is not "fmt")
        {
            throw new FormatException($"Attestation Statement's first key was '{label}' but 'fmt' was expected.");
        }

        state = cborReader.PeekState();
        if (state is not CborReaderState.TextString)
        {
            throw new FormatException($"Attestation Statement's first value was of type '{state}' but '{CborReaderState.TextString}' was expected.");
        }

        string fmt = cborReader.ReadTextString();
        if (fmt is not "packed")
        {
            throw new FormatException($"Attestation Statement had format '{fmt}' but 'packed' was expected.");
        }

        state = cborReader.PeekState();
        if (state is not CborReaderState.TextString)
        {
            throw new FormatException($"Attestation Statement's second key was of type '{state}' but '{CborReaderState.TextString}' was expected.");
        }

        label = cborReader.ReadTextString();
        if (label is not "attStmt")
        {
            throw new FormatException($"Attestation Statement's second key was '{label}' but 'attStmt' was expected.");
        }

        state = cborReader.PeekState();
        if (state is not CborReaderState.StartMap)
        {
            throw new FormatException($"Attestation Statement's 'attStmt' was of type '{state}' but '{CborReaderState.StartMap}' was expected.");
        }

        mapSize = cborReader.ReadStartMap();
        if (mapSize is not 2 or 3)
        {
            throw new FormatException($"Attestation Statement's packed format had '{mapSize}' entries but '2' or '3' was expected.");
        }

        state = cborReader.PeekState();
        if (state is not CborReaderState.TextString)
        {
            throw new FormatException($"Attestation Statement's packed format's first key was of type '{state}' but '{CborReaderState.TextString}' was expected.");
        }

        label = cborReader.ReadTextString();
        if (label is not "alg")
        {
            throw new FormatException($"Attestation Statement's packed format's first key was '{label}' but 'alg' was expected.");
        }

        state = cborReader.PeekState();
        if (state is not CborReaderState.NegativeInteger)
        {
            throw new FormatException($"Attestation Statement's packed format's 'alg' was of type '{state}' but '{CborReaderState.NegativeInteger}' was expected.");
        }

        ulong negativeAlg = cborReader.ReadCborNegativeIntegerRepresentation();

        state = cborReader.PeekState();
        if (state is not CborReaderState.TextString)
        {
            throw new FormatException($"Attestation Statement's packed format's second key was of type '{state}' but '{CborReaderState.TextString}' was expected.");
        }

        label = cborReader.ReadTextString();
        if (label is not "sig")
        {
            throw new FormatException($"Attestation Statement's packed format's second key was '{label}' but 'sig' was expected.");
        }

        state = cborReader.PeekState();
        if (state is not CborReaderState.ByteString)
        {
            throw new FormatException($"Attestation Statement's packed format's 'sig' was of type '{state}' but '{CborReaderState.ByteString}' was expected.");
        }

        byte[] signature = cborReader.ReadByteString();

        if (mapSize is 2)
        {
            return new()
            {
                Algorithm = (COSEAlgorithm)(-(long)negativeAlg - 1),
                Signature = signature,
            };
        }

        throw new NotSupportedException("Reading x5c is not yet supported.");
    }
}
