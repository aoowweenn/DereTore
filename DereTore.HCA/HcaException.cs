﻿using System;

namespace DereTore.HCA {
    public sealed class HcaException : Exception {

        public HcaException(string message, ActionResult actionResult)
            : base(message) {
            _actionResult = actionResult;
        }

        public HcaException(string message, ActionResult actionResult, Exception innerException)
            : base(message, innerException) {
            _actionResult = actionResult;
        }

        public ActionResult ActionResult => _actionResult;

        private readonly ActionResult _actionResult;

    }
}
