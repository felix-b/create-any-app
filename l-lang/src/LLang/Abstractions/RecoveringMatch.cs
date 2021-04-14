namespace LLang.Abstractions
{
    public class RecoveringMatch<TIn, TOut> : IMatch<TIn, TOut>
    {
        private enum RecoveryState
        {
            MainMatchInProgress = 0,
            MainMatchSuccess = 1,
            RecoveryMatchInProgress = 2,
            RecoveryMatchFailed = 3
        }
        
        private readonly IMatch<TIn, TOut> _mainMatch;
        private readonly Rule<TIn, TOut> _recoveryRule;
        private RecoveryState _state;
        private IRuleMatch<TIn, TOut>? _recoveryMatch;
        private OptionalProduct<TOut> _product;

        public RecoveringMatch(IMatch<TIn, TOut> mainMatch, Rule<TIn, TOut> recoveryRule)
        {
            _mainMatch = mainMatch;
            _recoveryRule = recoveryRule;
            _state = RecoveryState.MainMatchInProgress;
        }

        public bool Next(IInputContext<TIn> context)
        {
            if (_recoveryMatch != null)
            {
                return _recoveryMatch.Next(context);
            }

            if (_mainMatch.Next(context))
            {
                return true;
            }
            
            if (_mainMatch.ValidateMatch(context))
            {
                _state = RecoveryState.MainMatchSuccess;
                return false;
            }
            
            _recoveryMatch = _recoveryRule.TryMatchStart(context);
            _state = (_recoveryMatch != null
                ? RecoveryState.RecoveryMatchInProgress
                : RecoveryState.RecoveryMatchFailed);

            return (_state == RecoveryState.RecoveryMatchInProgress);
        }

        public bool ValidateMatch(IInputContext<TIn> context)
        {
            if (_state == RecoveryState.MainMatchSuccess)
            {
                return true;
            }

            if (_recoveryMatch != null)
            {
                var recovered = _recoveryMatch.ValidateMatch(context);
                if (recovered)
                {
                    _product = OptionalProduct.WithValue(
                        _recoveryRule.ProductFactory.Create(_recoveryMatch, context)
                    );
                }
                return recovered;
            }

            return _mainMatch.ValidateMatch(context);
        }

        public Marker<TIn> StartMarker => _mainMatch.StartMarker;

        public Marker<TIn> EndMarker => (_recoveryMatch != null
            ? _mainMatch.EndMarker
            : _recoveryMatch?.EndMarker ?? new Marker<TIn>());

        public OptionalProduct<TOut> Product => _product.HasValue 
            ? _product 
            : _mainMatch.Product;
    }
}