﻿using CERTENROLLLib;
using PKI.Utils;
using System;
using System.DirectoryServices;
using System.Security.Cryptography;
using System.Text;

namespace PKI.CertificateTemplates {
	/// <summary>
	/// Represents certificate template key archival settings.
	/// </summary>
	public class KeyArchivalOptions {
		readonly DirectoryEntry entry;

		internal KeyArchivalOptions(DirectoryEntry Entry) {
			entry = Entry;
			InitializeDs();
		}
		internal KeyArchivalOptions (IX509CertificateTemplate template) {
			InitializeCom(template);
		}

		/// <summary>
		/// Specifies whether the key archival is required for the template.
		/// </summary>
		public Boolean KeyArchival { get; private set; }
		/// <summary>
		/// Gets the encryption symmetric algorithm.
		/// </summary>
		public Oid EncryptionAlgorithm { get; private set; }
		/// <summary>
		/// Gets symmetric key length
		/// </summary>
		public Int32 KeyLength { get; private set; }

		void InitializeDs() {
			if (((Int32)entry.Properties["msPKI-Private-Key-Flag"].Value & (Int32)PrivateKeyFlags.RequireKeyArchival) > 0) {
				KeyArchival = true;
				String ap = (String)entry.Properties["msPKI-RA-Application-Policies"].Value;
				if (ap != null && ap.Contains("`")) {
					String[] splitstring = { "`" };
					String[] strings = ap.Split(splitstring, StringSplitOptions.RemoveEmptyEntries);
					for (Int32 index = 0; index < strings.Length; index += 3) {
						switch (strings[index]) {
							case "msPKI-Symmetric-Algorithm": EncryptionAlgorithm = new Oid(strings[index + 2]); break;
							case "msPKI-Symmetric-Key-Length": KeyLength = Convert.ToInt32(strings[index + 2]); break;
						}
					}
				}
			}
		}
		void InitializeCom(IX509CertificateTemplate template) {
			if (CryptographyUtils.TestOleCompat()) {
				if (((Int32)template.Property[EnrollmentTemplateProperty.TemplatePropPrivateKeyFlags] & (Int32)PrivateKeyFlags.RequireKeyArchival) > 0) {
					KeyArchival = true;
					try {
						IObjectId soid = (IObjectId)template.Property[EnrollmentTemplateProperty.TemplatePropSymmetricAlgorithm];
						EncryptionAlgorithm = new Oid(soid.Value);
					} catch { }
					try {
						KeyLength = (Int32)template.Property[EnrollmentTemplateProperty.TemplatePropSymmetricKeyLength];
					} catch { }
				}
			} else {
				if (((UInt32)template.Property[EnrollmentTemplateProperty.TemplatePropPrivateKeyFlags] & (Int32)PrivateKeyFlags.RequireKeyArchival) > 0) {
					KeyArchival = true;
					try {
						IObjectId soid = (IObjectId)template.Property[EnrollmentTemplateProperty.TemplatePropSymmetricAlgorithm];
						EncryptionAlgorithm = new Oid(soid.Value);
					} catch { }
					try {
						KeyLength = Convert.ToInt32(template.Property[EnrollmentTemplateProperty.TemplatePropSymmetricKeyLength]);
					} catch { }
				}
			}
		}

		/// <summary>
		/// Represents the current object in a textual form.
		/// </summary>
		/// <returns>Textual representation of the object.</returns>
		public override String ToString() {
			StringBuilder SB = new StringBuilder();
			SB.Append("[Key Archival Settings]" + Environment.NewLine);
			SB.Append("  Key archival required: " + KeyArchival);
			if (KeyArchival) {
				SB.Append(Environment.NewLine);
				SB.Append("  Symmetric algorithm: " + EncryptionAlgorithm.FriendlyName);
				SB.Append(Environment.NewLine);
				SB.Append("  Symmetric key length: " + KeyLength);
			}
			return SB.ToString();
		}
	}
}
