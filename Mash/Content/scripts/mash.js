/**
 * mash.js
 *
 *   Set up a *mash environment
 *
 * Licensed under the MIT:
 * http://www.opensource.org/licenses/mit-license.php
 *
 * @author Pat Newell (@pnewell4)
 * @version 0.1
 * @link 
*/

;(function($, document, window, undefined) {
	var CONSTANTS = {
		DATAKEY_OPTION: '__mash_options',
		DATAKEY_MEDIA: '$media'
	};

	// this is the magic, call $jqueryObj.mash() with either a method or 
	$.fn.mash = function(method) {
		if (methods[method]) {
			return methods[method].apply(this, Array.prototype.slice.call(arguments, 1));
		} else if (typeof method === 'object' || ! method) {
			return methods.init.apply(this, arguments);
		} else {
			$.error('Method ' +  method + ' does not exist on jQuery.GCProfile');
		}
		return null;
	};

	$.fn.mash.defaults = {
		debug: true,
		leaderboardSelector: "#leaderboard",
		leaderboardUrl: "/leaderboard/",
		leaderboardCount: 10,
		leaderboardItemSelector: ".leader-media",
		mashSelector: ".mash-display",
		mediaWrapper: ".media-wrapper",
		mediaSelector: ".media",
		sameSelector: ".same-media",
		submitButton: "input[type='submit'], button[type='submit']",
		mediaDisplay: function($media){

		},
		mediaChooseCallback: function($mash, $winner, $losers, options){

		},
		completeClassname: "mashed",    // gets added to mashSelector when complete
		winnerClassname: "mash-winner", // gets added to the winning mediaSelector
		losersClassname: "mash-losers",  // gets added to all losing mediaSelector(s)
		activeMashClass: "active-mash",

	};

	var methods = {
		init: function(options) {
			var options = $.extend({}, $.fn.mash.defaults, options);
			
			var r = this.each(function() {
				// Set up basic vars
				var o = options,
				    $this = $(this);

				$(this).data(CONSTANTS.DATAKEY_OPTION, o);
				// Start Plugin here
				$this.on({
					click: function(){
						$(o.submitButton, $(this)).trigger("click");
					}
				}, o.mediaWrapper);
				$this.on({
					mashSetup: function(){
						if(o.mediaDisplay && typeof o.mediaDisplay === 'function'){
							o.mediaDisplay($(this));
						}
					}
				}, o.mediaSelector);
				$this.on({
					click: function(e){
						var $this = $(this),
						    $form = $this.closest("form"),
						    formData = $form.serialize() || "";

						if(!$this.closest(o.mashSelector).length)
							return;

						e.stopPropagation();

						if(formData.length)
							formData += '&';
						formData += $this.attr("name") + "=" + $this.val();

						$.ajax({
							type: "post",
							url: $form.attr("action"),
							data: formData,
							success: function(){
								var $winner = $this.closest(o.mediaWrapper),
								    $mash = $this.closest(o.mashSelector),
								    $losers = $(o.mediaWrapper, $mash).not($winner);

								$winner.addClass(o.winnerClassname);
								$losers.addClass(o.losersClassname);
								$mash.addClass(o.completeClassname);

								var $allMashes = $(o.mashSelector),
								    mashInx = $allMashes.index($mash),
								    $nextMash = $allMashes.eq(mashInx+1);

								$allMashes.removeClass(o.activeMashClass);
								$nextMash.addClass(o.activeMashClass);

								if(typeof o.mediaChooseCallback === "function"){
									o.mediaChooseCallback($mash, $winner, $losers, o);
								}
							}
						});

						return false;
					}
				}, o.submitButton);
				var $getActiveMash = function(){
					return $(o.mashSelector + "." + o.activeMashClass + ":first");
				};
				var keystrokeSelect = function(isLeft){
					var $activeMash = $getActiveMash(),
					    $submits = $(o.submitButton, $activeMash);

					if($submits.length < 2) return;

					if(isLeft)
						$submits.first().trigger("click");
					else
						$submits.last().trigger("click");
				}
				var toggleSame = function(){
					var $activeMash = $getActiveMash(),
					    $same = $(o.sameSelector, $activeMash);

					$same.prop("checked", !$same.prop("checked"));
				};
				$this.on({
					keyup: function(e){
						switch(e.keyCode){
							case 32: //space
								toggleSame();
								e.stopPropagation();
								return false;
								break;
							case 37: //left
								keystrokeSelect(true);
								e.stopPropagation();
								break;
							case 39: //right
								keystrokeSelect(false);
								e.stopPropagation();
								break;
						}
					}
				});

				$(o.mediaSelector, $this).trigger("mashSetup"); // since these have probably already loadaed
				$(o.mashSelector, $this).removeClass(o.activeMashClass).first().addClass(o.activeMashClass);
			});

			this.mash("leaderboard");
			return r;
		},
		leaderboard: function(){
			return this.each(function() {
				// Set up basic vars
				var $this = $(this),
				    o = $this.data(CONSTANTS.DATAKEY_OPTION);
				$(o.leaderboardSelector).empty().load(o.leaderboardUrl + o.leaderboardCount);
			});
		}
	};
}) (jQuery, document, window);
