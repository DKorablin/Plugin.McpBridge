//#region \0rolldown/runtime.js
var e = Object.defineProperty, t = (t, n) => {
	let r = {};
	for (var i in t) e(r, i, {
		get: t[i],
		enumerable: !0
	});
	return n || e(r, Symbol.toStringTag, { value: "Module" }), r;
}, n = [];
for (let e = 0; e < 256; ++e) n.push((e + 256).toString(16).slice(1));
function r(e, t = 0) {
	return (n[e[t + 0]] + n[e[t + 1]] + n[e[t + 2]] + n[e[t + 3]] + "-" + n[e[t + 4]] + n[e[t + 5]] + "-" + n[e[t + 6]] + n[e[t + 7]] + "-" + n[e[t + 8]] + n[e[t + 9]] + "-" + n[e[t + 10]] + n[e[t + 11]] + n[e[t + 12]] + n[e[t + 13]] + n[e[t + 14]] + n[e[t + 15]]).toLowerCase();
}
//#endregion
//#region node_modules/uuid/dist/esm-browser/rng.js
var i, a = new Uint8Array(16);
function o() {
	if (!i) {
		if (typeof crypto > "u" || !crypto.getRandomValues) throw Error("crypto.getRandomValues() not supported. See https://github.com/uuidjs/uuid#getrandomvalues-not-supported");
		i = crypto.getRandomValues.bind(crypto);
	}
	return i(a);
}
var s = { randomUUID: typeof crypto < "u" && crypto.randomUUID && crypto.randomUUID.bind(crypto) };
//#endregion
//#region node_modules/uuid/dist/esm-browser/v4.js
function c(e, t, n) {
	if (s.randomUUID && !t && !e) return s.randomUUID();
	e ||= {};
	let i = e.random ?? e.rng?.() ?? o();
	if (i.length < 16) throw Error("Random bytes length must be >= 16");
	if (i[6] = i[6] & 15 | 64, i[8] = i[8] & 63 | 128, t) {
		if (n ||= 0, n < 0 || n + 16 > t.length) throw RangeError(`UUID byte range ${n}:${n + 15} is out of buffer bounds`);
		for (let e = 0; e < 16; ++e) t[n + e] = i[e];
		return t;
	}
	return r(i);
}
//#endregion
//#region node_modules/zod/v3/helpers/util.js
var l;
(function(e) {
	e.assertEqual = (e) => {};
	function t(e) {}
	e.assertIs = t;
	function n(e) {
		throw Error();
	}
	e.assertNever = n, e.arrayToEnum = (e) => {
		let t = {};
		for (let n of e) t[n] = n;
		return t;
	}, e.getValidEnumValues = (t) => {
		let n = e.objectKeys(t).filter((e) => typeof t[t[e]] != "number"), r = {};
		for (let e of n) r[e] = t[e];
		return e.objectValues(r);
	}, e.objectValues = (t) => e.objectKeys(t).map(function(e) {
		return t[e];
	}), e.objectKeys = typeof Object.keys == "function" ? (e) => Object.keys(e) : (e) => {
		let t = [];
		for (let n in e) Object.prototype.hasOwnProperty.call(e, n) && t.push(n);
		return t;
	}, e.find = (e, t) => {
		for (let n of e) if (t(n)) return n;
	}, e.isInteger = typeof Number.isInteger == "function" ? (e) => Number.isInteger(e) : (e) => typeof e == "number" && Number.isFinite(e) && Math.floor(e) === e;
	function r(e, t = " | ") {
		return e.map((e) => typeof e == "string" ? `'${e}'` : e).join(t);
	}
	e.joinValues = r, e.jsonStringifyReplacer = (e, t) => typeof t == "bigint" ? t.toString() : t;
})(l ||= {});
var u;
(function(e) {
	e.mergeShapes = (e, t) => ({
		...e,
		...t
	});
})(u ||= {});
var d = l.arrayToEnum([
	"string",
	"nan",
	"number",
	"integer",
	"float",
	"boolean",
	"date",
	"bigint",
	"symbol",
	"function",
	"undefined",
	"null",
	"array",
	"object",
	"unknown",
	"promise",
	"void",
	"never",
	"map",
	"set"
]), f = (e) => {
	switch (typeof e) {
		case "undefined": return d.undefined;
		case "string": return d.string;
		case "number": return Number.isNaN(e) ? d.nan : d.number;
		case "boolean": return d.boolean;
		case "function": return d.function;
		case "bigint": return d.bigint;
		case "symbol": return d.symbol;
		case "object": return Array.isArray(e) ? d.array : e === null ? d.null : e.then && typeof e.then == "function" && e.catch && typeof e.catch == "function" ? d.promise : typeof Map < "u" && e instanceof Map ? d.map : typeof Set < "u" && e instanceof Set ? d.set : typeof Date < "u" && e instanceof Date ? d.date : d.object;
		default: return d.unknown;
	}
}, p = l.arrayToEnum([
	"invalid_type",
	"invalid_literal",
	"custom",
	"invalid_union",
	"invalid_union_discriminator",
	"invalid_enum_value",
	"unrecognized_keys",
	"invalid_arguments",
	"invalid_return_type",
	"invalid_date",
	"invalid_string",
	"too_small",
	"too_big",
	"invalid_intersection_types",
	"not_multiple_of",
	"not_finite"
]), m = class e extends Error {
	get errors() {
		return this.issues;
	}
	constructor(e) {
		super(), this.issues = [], this.addIssue = (e) => {
			this.issues = [...this.issues, e];
		}, this.addIssues = (e = []) => {
			this.issues = [...this.issues, ...e];
		};
		let t = new.target.prototype;
		Object.setPrototypeOf ? Object.setPrototypeOf(this, t) : this.__proto__ = t, this.name = "ZodError", this.issues = e;
	}
	format(e) {
		let t = e || function(e) {
			return e.message;
		}, n = { _errors: [] }, r = (e) => {
			for (let i of e.issues) if (i.code === "invalid_union") i.unionErrors.map(r);
			else if (i.code === "invalid_return_type") r(i.returnTypeError);
			else if (i.code === "invalid_arguments") r(i.argumentsError);
			else if (i.path.length === 0) n._errors.push(t(i));
			else {
				let e = n, r = 0;
				for (; r < i.path.length;) {
					let n = i.path[r];
					r === i.path.length - 1 ? (e[n] = e[n] || { _errors: [] }, e[n]._errors.push(t(i))) : e[n] = e[n] || { _errors: [] }, e = e[n], r++;
				}
			}
		};
		return r(this), n;
	}
	static assert(t) {
		if (!(t instanceof e)) throw Error(`Not a ZodError: ${t}`);
	}
	toString() {
		return this.message;
	}
	get message() {
		return JSON.stringify(this.issues, l.jsonStringifyReplacer, 2);
	}
	get isEmpty() {
		return this.issues.length === 0;
	}
	flatten(e = (e) => e.message) {
		let t = {}, n = [];
		for (let r of this.issues) if (r.path.length > 0) {
			let n = r.path[0];
			t[n] = t[n] || [], t[n].push(e(r));
		} else n.push(e(r));
		return {
			formErrors: n,
			fieldErrors: t
		};
	}
	get formErrors() {
		return this.flatten();
	}
};
m.create = (e) => new m(e);
//#endregion
//#region node_modules/zod/v3/locales/en.js
var h = (e, t) => {
	let n;
	switch (e.code) {
		case p.invalid_type:
			n = e.received === d.undefined ? "Required" : `Expected ${e.expected}, received ${e.received}`;
			break;
		case p.invalid_literal:
			n = `Invalid literal value, expected ${JSON.stringify(e.expected, l.jsonStringifyReplacer)}`;
			break;
		case p.unrecognized_keys:
			n = `Unrecognized key(s) in object: ${l.joinValues(e.keys, ", ")}`;
			break;
		case p.invalid_union:
			n = "Invalid input";
			break;
		case p.invalid_union_discriminator:
			n = `Invalid discriminator value. Expected ${l.joinValues(e.options)}`;
			break;
		case p.invalid_enum_value:
			n = `Invalid enum value. Expected ${l.joinValues(e.options)}, received '${e.received}'`;
			break;
		case p.invalid_arguments:
			n = "Invalid function arguments";
			break;
		case p.invalid_return_type:
			n = "Invalid function return type";
			break;
		case p.invalid_date:
			n = "Invalid date";
			break;
		case p.invalid_string:
			typeof e.validation == "object" ? "includes" in e.validation ? (n = `Invalid input: must include "${e.validation.includes}"`, typeof e.validation.position == "number" && (n = `${n} at one or more positions greater than or equal to ${e.validation.position}`)) : "startsWith" in e.validation ? n = `Invalid input: must start with "${e.validation.startsWith}"` : "endsWith" in e.validation ? n = `Invalid input: must end with "${e.validation.endsWith}"` : l.assertNever(e.validation) : n = e.validation === "regex" ? "Invalid" : `Invalid ${e.validation}`;
			break;
		case p.too_small:
			n = e.type === "array" ? `Array must contain ${e.exact ? "exactly" : e.inclusive ? "at least" : "more than"} ${e.minimum} element(s)` : e.type === "string" ? `String must contain ${e.exact ? "exactly" : e.inclusive ? "at least" : "over"} ${e.minimum} character(s)` : e.type === "number" || e.type === "bigint" ? `Number must be ${e.exact ? "exactly equal to " : e.inclusive ? "greater than or equal to " : "greater than "}${e.minimum}` : e.type === "date" ? `Date must be ${e.exact ? "exactly equal to " : e.inclusive ? "greater than or equal to " : "greater than "}${new Date(Number(e.minimum))}` : "Invalid input";
			break;
		case p.too_big:
			n = e.type === "array" ? `Array must contain ${e.exact ? "exactly" : e.inclusive ? "at most" : "less than"} ${e.maximum} element(s)` : e.type === "string" ? `String must contain ${e.exact ? "exactly" : e.inclusive ? "at most" : "under"} ${e.maximum} character(s)` : e.type === "number" ? `Number must be ${e.exact ? "exactly" : e.inclusive ? "less than or equal to" : "less than"} ${e.maximum}` : e.type === "bigint" ? `BigInt must be ${e.exact ? "exactly" : e.inclusive ? "less than or equal to" : "less than"} ${e.maximum}` : e.type === "date" ? `Date must be ${e.exact ? "exactly" : e.inclusive ? "smaller than or equal to" : "smaller than"} ${new Date(Number(e.maximum))}` : "Invalid input";
			break;
		case p.custom:
			n = "Invalid input";
			break;
		case p.invalid_intersection_types:
			n = "Intersection results could not be merged";
			break;
		case p.not_multiple_of:
			n = `Number must be a multiple of ${e.multipleOf}`;
			break;
		case p.not_finite:
			n = "Number must be finite";
			break;
		default: n = t.defaultError, l.assertNever(e);
	}
	return { message: n };
}, ee = h;
function te() {
	return ee;
}
//#endregion
//#region node_modules/zod/v3/helpers/parseUtil.js
var ne = (e) => {
	let { data: t, path: n, errorMaps: r, issueData: i } = e, a = [...n, ...i.path || []], o = {
		...i,
		path: a
	};
	if (i.message !== void 0) return {
		...i,
		path: a,
		message: i.message
	};
	let s = "", c = r.filter((e) => !!e).slice().reverse();
	for (let e of c) s = e(o, {
		data: t,
		defaultError: s
	}).message;
	return {
		...i,
		path: a,
		message: s
	};
};
function g(e, t) {
	let n = te(), r = ne({
		issueData: t,
		data: e.data,
		path: e.path,
		errorMaps: [
			e.common.contextualErrorMap,
			e.schemaErrorMap,
			n,
			n === h ? void 0 : h
		].filter((e) => !!e)
	});
	e.common.issues.push(r);
}
var _ = class e {
	constructor() {
		this.value = "valid";
	}
	dirty() {
		this.value === "valid" && (this.value = "dirty");
	}
	abort() {
		this.value !== "aborted" && (this.value = "aborted");
	}
	static mergeArray(e, t) {
		let n = [];
		for (let r of t) {
			if (r.status === "aborted") return v;
			r.status === "dirty" && e.dirty(), n.push(r.value);
		}
		return {
			status: e.value,
			value: n
		};
	}
	static async mergeObjectAsync(t, n) {
		let r = [];
		for (let e of n) {
			let t = await e.key, n = await e.value;
			r.push({
				key: t,
				value: n
			});
		}
		return e.mergeObjectSync(t, r);
	}
	static mergeObjectSync(e, t) {
		let n = {};
		for (let r of t) {
			let { key: t, value: i } = r;
			if (t.status === "aborted" || i.status === "aborted") return v;
			t.status === "dirty" && e.dirty(), i.status === "dirty" && e.dirty(), t.value !== "__proto__" && (i.value !== void 0 || r.alwaysSet) && (n[t.value] = i.value);
		}
		return {
			status: e.value,
			value: n
		};
	}
}, v = Object.freeze({ status: "aborted" }), re = (e) => ({
	status: "dirty",
	value: e
}), y = (e) => ({
	status: "valid",
	value: e
}), ie = (e) => e.status === "aborted", ae = (e) => e.status === "dirty", oe = (e) => e.status === "valid", se = (e) => typeof Promise < "u" && e instanceof Promise, b;
(function(e) {
	e.errToObj = (e) => typeof e == "string" ? { message: e } : e || {}, e.toString = (e) => typeof e == "string" ? e : e?.message;
})(b ||= {});
//#endregion
//#region node_modules/zod/v3/types.js
var x = class {
	constructor(e, t, n, r) {
		this._cachedPath = [], this.parent = e, this.data = t, this._path = n, this._key = r;
	}
	get path() {
		return this._cachedPath.length || (Array.isArray(this._key) ? this._cachedPath.push(...this._path, ...this._key) : this._cachedPath.push(...this._path, this._key)), this._cachedPath;
	}
}, ce = (e, t) => {
	if (oe(t)) return {
		success: !0,
		data: t.value
	};
	if (!e.common.issues.length) throw Error("Validation failed but no issues detected.");
	return {
		success: !1,
		get error() {
			if (this._error) return this._error;
			let t = new m(e.common.issues);
			return this._error = t, this._error;
		}
	};
};
function S(e) {
	if (!e) return {};
	let { errorMap: t, invalid_type_error: n, required_error: r, description: i } = e;
	if (t && (n || r)) throw Error("Can't use \"invalid_type_error\" or \"required_error\" in conjunction with custom error map.");
	return t ? {
		errorMap: t,
		description: i
	} : {
		errorMap: (t, i) => {
			let { message: a } = e;
			return t.code === "invalid_enum_value" ? { message: a ?? i.defaultError } : i.data === void 0 ? { message: a ?? r ?? i.defaultError } : t.code === "invalid_type" ? { message: a ?? n ?? i.defaultError } : { message: i.defaultError };
		},
		description: i
	};
}
var C = class {
	get description() {
		return this._def.description;
	}
	_getType(e) {
		return f(e.data);
	}
	_getOrReturnCtx(e, t) {
		return t || {
			common: e.parent.common,
			data: e.data,
			parsedType: f(e.data),
			schemaErrorMap: this._def.errorMap,
			path: e.path,
			parent: e.parent
		};
	}
	_processInputParams(e) {
		return {
			status: new _(),
			ctx: {
				common: e.parent.common,
				data: e.data,
				parsedType: f(e.data),
				schemaErrorMap: this._def.errorMap,
				path: e.path,
				parent: e.parent
			}
		};
	}
	_parseSync(e) {
		let t = this._parse(e);
		if (se(t)) throw Error("Synchronous parse encountered promise.");
		return t;
	}
	_parseAsync(e) {
		let t = this._parse(e);
		return Promise.resolve(t);
	}
	parse(e, t) {
		let n = this.safeParse(e, t);
		if (n.success) return n.data;
		throw n.error;
	}
	safeParse(e, t) {
		let n = {
			common: {
				issues: [],
				async: t?.async ?? !1,
				contextualErrorMap: t?.errorMap
			},
			path: t?.path || [],
			schemaErrorMap: this._def.errorMap,
			parent: null,
			data: e,
			parsedType: f(e)
		};
		return ce(n, this._parseSync({
			data: e,
			path: n.path,
			parent: n
		}));
	}
	"~validate"(e) {
		let t = {
			common: {
				issues: [],
				async: !!this["~standard"].async
			},
			path: [],
			schemaErrorMap: this._def.errorMap,
			parent: null,
			data: e,
			parsedType: f(e)
		};
		if (!this["~standard"].async) try {
			let n = this._parseSync({
				data: e,
				path: [],
				parent: t
			});
			return oe(n) ? { value: n.value } : { issues: t.common.issues };
		} catch (e) {
			e?.message?.toLowerCase()?.includes("encountered") && (this["~standard"].async = !0), t.common = {
				issues: [],
				async: !0
			};
		}
		return this._parseAsync({
			data: e,
			path: [],
			parent: t
		}).then((e) => oe(e) ? { value: e.value } : { issues: t.common.issues });
	}
	async parseAsync(e, t) {
		let n = await this.safeParseAsync(e, t);
		if (n.success) return n.data;
		throw n.error;
	}
	async safeParseAsync(e, t) {
		let n = {
			common: {
				issues: [],
				contextualErrorMap: t?.errorMap,
				async: !0
			},
			path: t?.path || [],
			schemaErrorMap: this._def.errorMap,
			parent: null,
			data: e,
			parsedType: f(e)
		}, r = this._parse({
			data: e,
			path: n.path,
			parent: n
		});
		return ce(n, await (se(r) ? r : Promise.resolve(r)));
	}
	refine(e, t) {
		let n = (e) => typeof t == "string" || t === void 0 ? { message: t } : typeof t == "function" ? t(e) : t;
		return this._refinement((t, r) => {
			let i = e(t), a = () => r.addIssue({
				code: p.custom,
				...n(t)
			});
			return typeof Promise < "u" && i instanceof Promise ? i.then((e) => e ? !0 : (a(), !1)) : i ? !0 : (a(), !1);
		});
	}
	refinement(e, t) {
		return this._refinement((n, r) => e(n) ? !0 : (r.addIssue(typeof t == "function" ? t(n, r) : t), !1));
	}
	_refinement(e) {
		return new T({
			schema: this,
			typeName: D.ZodEffects,
			effect: {
				type: "refinement",
				refinement: e
			}
		});
	}
	superRefine(e) {
		return this._refinement(e);
	}
	constructor(e) {
		this.spa = this.safeParseAsync, this._def = e, this.parse = this.parse.bind(this), this.safeParse = this.safeParse.bind(this), this.parseAsync = this.parseAsync.bind(this), this.safeParseAsync = this.safeParseAsync.bind(this), this.spa = this.spa.bind(this), this.refine = this.refine.bind(this), this.refinement = this.refinement.bind(this), this.superRefine = this.superRefine.bind(this), this.optional = this.optional.bind(this), this.nullable = this.nullable.bind(this), this.nullish = this.nullish.bind(this), this.array = this.array.bind(this), this.promise = this.promise.bind(this), this.or = this.or.bind(this), this.and = this.and.bind(this), this.transform = this.transform.bind(this), this.brand = this.brand.bind(this), this.default = this.default.bind(this), this.catch = this.catch.bind(this), this.describe = this.describe.bind(this), this.pipe = this.pipe.bind(this), this.readonly = this.readonly.bind(this), this.isNullable = this.isNullable.bind(this), this.isOptional = this.isOptional.bind(this), this["~standard"] = {
			version: 1,
			vendor: "zod",
			validate: (e) => this["~validate"](e)
		};
	}
	optional() {
		return E.create(this, this._def);
	}
	nullable() {
		return ut.create(this, this._def);
	}
	nullish() {
		return this.nullable().optional();
	}
	array() {
		return Ke.create(this);
	}
	promise() {
		return lt.create(this, this._def);
	}
	or(e) {
		return Je.create([this, e], this._def);
	}
	and(e) {
		return Qe.create(this, e, this._def);
	}
	transform(e) {
		return new T({
			...S(this._def),
			schema: this,
			typeName: D.ZodEffects,
			effect: {
				type: "transform",
				transform: e
			}
		});
	}
	default(e) {
		let t = typeof e == "function" ? e : () => e;
		return new dt({
			...S(this._def),
			innerType: this,
			defaultValue: t,
			typeName: D.ZodDefault
		});
	}
	brand() {
		return new mt({
			typeName: D.ZodBranded,
			type: this,
			...S(this._def)
		});
	}
	catch(e) {
		let t = typeof e == "function" ? e : () => e;
		return new ft({
			...S(this._def),
			innerType: this,
			catchValue: t,
			typeName: D.ZodCatch
		});
	}
	describe(e) {
		let t = this.constructor;
		return new t({
			...this._def,
			description: e
		});
	}
	pipe(e) {
		return ht.create(this, e);
	}
	readonly() {
		return gt.create(this);
	}
	isOptional() {
		return this.safeParse(void 0).success;
	}
	isNullable() {
		return this.safeParse(null).success;
	}
}, le = /^c[^\s-]{8,}$/i, ue = /^[0-9a-z]+$/, de = /^[0-9A-HJKMNP-TV-Z]{26}$/i, fe = /^[0-9a-fA-F]{8}\b-[0-9a-fA-F]{4}\b-[0-9a-fA-F]{4}\b-[0-9a-fA-F]{4}\b-[0-9a-fA-F]{12}$/i, pe = /^[a-z0-9_-]{21}$/i, me = /^[A-Za-z0-9-_]+\.[A-Za-z0-9-_]+\.[A-Za-z0-9-_]*$/, he = /^[-+]?P(?!$)(?:(?:[-+]?\d+Y)|(?:[-+]?\d+[.,]\d+Y$))?(?:(?:[-+]?\d+M)|(?:[-+]?\d+[.,]\d+M$))?(?:(?:[-+]?\d+W)|(?:[-+]?\d+[.,]\d+W$))?(?:(?:[-+]?\d+D)|(?:[-+]?\d+[.,]\d+D$))?(?:T(?=[\d+-])(?:(?:[-+]?\d+H)|(?:[-+]?\d+[.,]\d+H$))?(?:(?:[-+]?\d+M)|(?:[-+]?\d+[.,]\d+M$))?(?:[-+]?\d+(?:[.,]\d+)?S)?)??$/, ge = /^(?!\.)(?!.*\.\.)([A-Z0-9_'+\-\.]*)[A-Z0-9_+-]@([A-Z0-9][A-Z0-9\-]*\.)+[A-Z]{2,}$/i, _e = "^(\\p{Extended_Pictographic}|\\p{Emoji_Component})+$", ve, ye = /^(?:(?:25[0-5]|2[0-4][0-9]|1[0-9][0-9]|[1-9][0-9]|[0-9])\.){3}(?:25[0-5]|2[0-4][0-9]|1[0-9][0-9]|[1-9][0-9]|[0-9])$/, be = /^(?:(?:25[0-5]|2[0-4][0-9]|1[0-9][0-9]|[1-9][0-9]|[0-9])\.){3}(?:25[0-5]|2[0-4][0-9]|1[0-9][0-9]|[1-9][0-9]|[0-9])\/(3[0-2]|[12]?[0-9])$/, xe = /^(([0-9a-fA-F]{1,4}:){7,7}[0-9a-fA-F]{1,4}|([0-9a-fA-F]{1,4}:){1,7}:|([0-9a-fA-F]{1,4}:){1,6}:[0-9a-fA-F]{1,4}|([0-9a-fA-F]{1,4}:){1,5}(:[0-9a-fA-F]{1,4}){1,2}|([0-9a-fA-F]{1,4}:){1,4}(:[0-9a-fA-F]{1,4}){1,3}|([0-9a-fA-F]{1,4}:){1,3}(:[0-9a-fA-F]{1,4}){1,4}|([0-9a-fA-F]{1,4}:){1,2}(:[0-9a-fA-F]{1,4}){1,5}|[0-9a-fA-F]{1,4}:((:[0-9a-fA-F]{1,4}){1,6})|:((:[0-9a-fA-F]{1,4}){1,7}|:)|fe80:(:[0-9a-fA-F]{0,4}){0,4}%[0-9a-zA-Z]{1,}|::(ffff(:0{1,4}){0,1}:){0,1}((25[0-5]|(2[0-4]|1{0,1}[0-9]){0,1}[0-9])\.){3,3}(25[0-5]|(2[0-4]|1{0,1}[0-9]){0,1}[0-9])|([0-9a-fA-F]{1,4}:){1,4}:((25[0-5]|(2[0-4]|1{0,1}[0-9]){0,1}[0-9])\.){3,3}(25[0-5]|(2[0-4]|1{0,1}[0-9]){0,1}[0-9]))$/, Se = /^(([0-9a-fA-F]{1,4}:){7,7}[0-9a-fA-F]{1,4}|([0-9a-fA-F]{1,4}:){1,7}:|([0-9a-fA-F]{1,4}:){1,6}:[0-9a-fA-F]{1,4}|([0-9a-fA-F]{1,4}:){1,5}(:[0-9a-fA-F]{1,4}){1,2}|([0-9a-fA-F]{1,4}:){1,4}(:[0-9a-fA-F]{1,4}){1,3}|([0-9a-fA-F]{1,4}:){1,3}(:[0-9a-fA-F]{1,4}){1,4}|([0-9a-fA-F]{1,4}:){1,2}(:[0-9a-fA-F]{1,4}){1,5}|[0-9a-fA-F]{1,4}:((:[0-9a-fA-F]{1,4}){1,6})|:((:[0-9a-fA-F]{1,4}){1,7}|:)|fe80:(:[0-9a-fA-F]{0,4}){0,4}%[0-9a-zA-Z]{1,}|::(ffff(:0{1,4}){0,1}:){0,1}((25[0-5]|(2[0-4]|1{0,1}[0-9]){0,1}[0-9])\.){3,3}(25[0-5]|(2[0-4]|1{0,1}[0-9]){0,1}[0-9])|([0-9a-fA-F]{1,4}:){1,4}:((25[0-5]|(2[0-4]|1{0,1}[0-9]){0,1}[0-9])\.){3,3}(25[0-5]|(2[0-4]|1{0,1}[0-9]){0,1}[0-9]))\/(12[0-8]|1[01][0-9]|[1-9]?[0-9])$/, Ce = /^([0-9a-zA-Z+/]{4})*(([0-9a-zA-Z+/]{2}==)|([0-9a-zA-Z+/]{3}=))?$/, we = /^([0-9a-zA-Z-_]{4})*(([0-9a-zA-Z-_]{2}(==)?)|([0-9a-zA-Z-_]{3}(=)?))?$/, Te = "((\\d\\d[2468][048]|\\d\\d[13579][26]|\\d\\d0[48]|[02468][048]00|[13579][26]00)-02-29|\\d{4}-((0[13578]|1[02])-(0[1-9]|[12]\\d|3[01])|(0[469]|11)-(0[1-9]|[12]\\d|30)|(02)-(0[1-9]|1\\d|2[0-8])))", Ee = RegExp(`^${Te}$`);
function De(e) {
	let t = "[0-5]\\d";
	e.precision ? t = `${t}\\.\\d{${e.precision}}` : e.precision ?? (t = `${t}(\\.\\d+)?`);
	let n = e.precision ? "+" : "?";
	return `([01]\\d|2[0-3]):[0-5]\\d(:${t})${n}`;
}
function Oe(e) {
	return RegExp(`^${De(e)}$`);
}
function ke(e) {
	let t = `${Te}T${De(e)}`, n = [];
	return n.push(e.local ? "Z?" : "Z"), e.offset && n.push("([+-]\\d{2}:?\\d{2})"), t = `${t}(${n.join("|")})`, RegExp(`^${t}$`);
}
function Ae(e, t) {
	return !!((t === "v4" || !t) && ye.test(e) || (t === "v6" || !t) && xe.test(e));
}
function je(e, t) {
	if (!me.test(e)) return !1;
	try {
		let [n] = e.split(".");
		if (!n) return !1;
		let r = n.replace(/-/g, "+").replace(/_/g, "/").padEnd(n.length + (4 - n.length % 4) % 4, "="), i = JSON.parse(atob(r));
		return !(typeof i != "object" || !i || "typ" in i && i?.typ !== "JWT" || !i.alg || t && i.alg !== t);
	} catch {
		return !1;
	}
}
function Me(e, t) {
	return !!((t === "v4" || !t) && be.test(e) || (t === "v6" || !t) && Se.test(e));
}
var Ne = class e extends C {
	_parse(e) {
		if (this._def.coerce && (e.data = String(e.data)), this._getType(e) !== d.string) {
			let t = this._getOrReturnCtx(e);
			return g(t, {
				code: p.invalid_type,
				expected: d.string,
				received: t.parsedType
			}), v;
		}
		let t = new _(), n;
		for (let r of this._def.checks) if (r.kind === "min") e.data.length < r.value && (n = this._getOrReturnCtx(e, n), g(n, {
			code: p.too_small,
			minimum: r.value,
			type: "string",
			inclusive: !0,
			exact: !1,
			message: r.message
		}), t.dirty());
		else if (r.kind === "max") e.data.length > r.value && (n = this._getOrReturnCtx(e, n), g(n, {
			code: p.too_big,
			maximum: r.value,
			type: "string",
			inclusive: !0,
			exact: !1,
			message: r.message
		}), t.dirty());
		else if (r.kind === "length") {
			let i = e.data.length > r.value, a = e.data.length < r.value;
			(i || a) && (n = this._getOrReturnCtx(e, n), i ? g(n, {
				code: p.too_big,
				maximum: r.value,
				type: "string",
				inclusive: !0,
				exact: !0,
				message: r.message
			}) : a && g(n, {
				code: p.too_small,
				minimum: r.value,
				type: "string",
				inclusive: !0,
				exact: !0,
				message: r.message
			}), t.dirty());
		} else if (r.kind === "email") ge.test(e.data) || (n = this._getOrReturnCtx(e, n), g(n, {
			validation: "email",
			code: p.invalid_string,
			message: r.message
		}), t.dirty());
		else if (r.kind === "emoji") ve ||= new RegExp(_e, "u"), ve.test(e.data) || (n = this._getOrReturnCtx(e, n), g(n, {
			validation: "emoji",
			code: p.invalid_string,
			message: r.message
		}), t.dirty());
		else if (r.kind === "uuid") fe.test(e.data) || (n = this._getOrReturnCtx(e, n), g(n, {
			validation: "uuid",
			code: p.invalid_string,
			message: r.message
		}), t.dirty());
		else if (r.kind === "nanoid") pe.test(e.data) || (n = this._getOrReturnCtx(e, n), g(n, {
			validation: "nanoid",
			code: p.invalid_string,
			message: r.message
		}), t.dirty());
		else if (r.kind === "cuid") le.test(e.data) || (n = this._getOrReturnCtx(e, n), g(n, {
			validation: "cuid",
			code: p.invalid_string,
			message: r.message
		}), t.dirty());
		else if (r.kind === "cuid2") ue.test(e.data) || (n = this._getOrReturnCtx(e, n), g(n, {
			validation: "cuid2",
			code: p.invalid_string,
			message: r.message
		}), t.dirty());
		else if (r.kind === "ulid") de.test(e.data) || (n = this._getOrReturnCtx(e, n), g(n, {
			validation: "ulid",
			code: p.invalid_string,
			message: r.message
		}), t.dirty());
		else if (r.kind === "url") try {
			new URL(e.data);
		} catch {
			n = this._getOrReturnCtx(e, n), g(n, {
				validation: "url",
				code: p.invalid_string,
				message: r.message
			}), t.dirty();
		}
		else r.kind === "regex" ? (r.regex.lastIndex = 0, r.regex.test(e.data) || (n = this._getOrReturnCtx(e, n), g(n, {
			validation: "regex",
			code: p.invalid_string,
			message: r.message
		}), t.dirty())) : r.kind === "trim" ? e.data = e.data.trim() : r.kind === "includes" ? e.data.includes(r.value, r.position) || (n = this._getOrReturnCtx(e, n), g(n, {
			code: p.invalid_string,
			validation: {
				includes: r.value,
				position: r.position
			},
			message: r.message
		}), t.dirty()) : r.kind === "toLowerCase" ? e.data = e.data.toLowerCase() : r.kind === "toUpperCase" ? e.data = e.data.toUpperCase() : r.kind === "startsWith" ? e.data.startsWith(r.value) || (n = this._getOrReturnCtx(e, n), g(n, {
			code: p.invalid_string,
			validation: { startsWith: r.value },
			message: r.message
		}), t.dirty()) : r.kind === "endsWith" ? e.data.endsWith(r.value) || (n = this._getOrReturnCtx(e, n), g(n, {
			code: p.invalid_string,
			validation: { endsWith: r.value },
			message: r.message
		}), t.dirty()) : r.kind === "datetime" ? ke(r).test(e.data) || (n = this._getOrReturnCtx(e, n), g(n, {
			code: p.invalid_string,
			validation: "datetime",
			message: r.message
		}), t.dirty()) : r.kind === "date" ? Ee.test(e.data) || (n = this._getOrReturnCtx(e, n), g(n, {
			code: p.invalid_string,
			validation: "date",
			message: r.message
		}), t.dirty()) : r.kind === "time" ? Oe(r).test(e.data) || (n = this._getOrReturnCtx(e, n), g(n, {
			code: p.invalid_string,
			validation: "time",
			message: r.message
		}), t.dirty()) : r.kind === "duration" ? he.test(e.data) || (n = this._getOrReturnCtx(e, n), g(n, {
			validation: "duration",
			code: p.invalid_string,
			message: r.message
		}), t.dirty()) : r.kind === "ip" ? Ae(e.data, r.version) || (n = this._getOrReturnCtx(e, n), g(n, {
			validation: "ip",
			code: p.invalid_string,
			message: r.message
		}), t.dirty()) : r.kind === "jwt" ? je(e.data, r.alg) || (n = this._getOrReturnCtx(e, n), g(n, {
			validation: "jwt",
			code: p.invalid_string,
			message: r.message
		}), t.dirty()) : r.kind === "cidr" ? Me(e.data, r.version) || (n = this._getOrReturnCtx(e, n), g(n, {
			validation: "cidr",
			code: p.invalid_string,
			message: r.message
		}), t.dirty()) : r.kind === "base64" ? Ce.test(e.data) || (n = this._getOrReturnCtx(e, n), g(n, {
			validation: "base64",
			code: p.invalid_string,
			message: r.message
		}), t.dirty()) : r.kind === "base64url" ? we.test(e.data) || (n = this._getOrReturnCtx(e, n), g(n, {
			validation: "base64url",
			code: p.invalid_string,
			message: r.message
		}), t.dirty()) : l.assertNever(r);
		return {
			status: t.value,
			value: e.data
		};
	}
	_regex(e, t, n) {
		return this.refinement((t) => e.test(t), {
			validation: t,
			code: p.invalid_string,
			...b.errToObj(n)
		});
	}
	_addCheck(t) {
		return new e({
			...this._def,
			checks: [...this._def.checks, t]
		});
	}
	email(e) {
		return this._addCheck({
			kind: "email",
			...b.errToObj(e)
		});
	}
	url(e) {
		return this._addCheck({
			kind: "url",
			...b.errToObj(e)
		});
	}
	emoji(e) {
		return this._addCheck({
			kind: "emoji",
			...b.errToObj(e)
		});
	}
	uuid(e) {
		return this._addCheck({
			kind: "uuid",
			...b.errToObj(e)
		});
	}
	nanoid(e) {
		return this._addCheck({
			kind: "nanoid",
			...b.errToObj(e)
		});
	}
	cuid(e) {
		return this._addCheck({
			kind: "cuid",
			...b.errToObj(e)
		});
	}
	cuid2(e) {
		return this._addCheck({
			kind: "cuid2",
			...b.errToObj(e)
		});
	}
	ulid(e) {
		return this._addCheck({
			kind: "ulid",
			...b.errToObj(e)
		});
	}
	base64(e) {
		return this._addCheck({
			kind: "base64",
			...b.errToObj(e)
		});
	}
	base64url(e) {
		return this._addCheck({
			kind: "base64url",
			...b.errToObj(e)
		});
	}
	jwt(e) {
		return this._addCheck({
			kind: "jwt",
			...b.errToObj(e)
		});
	}
	ip(e) {
		return this._addCheck({
			kind: "ip",
			...b.errToObj(e)
		});
	}
	cidr(e) {
		return this._addCheck({
			kind: "cidr",
			...b.errToObj(e)
		});
	}
	datetime(e) {
		return typeof e == "string" ? this._addCheck({
			kind: "datetime",
			precision: null,
			offset: !1,
			local: !1,
			message: e
		}) : this._addCheck({
			kind: "datetime",
			precision: e?.precision === void 0 ? null : e?.precision,
			offset: e?.offset ?? !1,
			local: e?.local ?? !1,
			...b.errToObj(e?.message)
		});
	}
	date(e) {
		return this._addCheck({
			kind: "date",
			message: e
		});
	}
	time(e) {
		return typeof e == "string" ? this._addCheck({
			kind: "time",
			precision: null,
			message: e
		}) : this._addCheck({
			kind: "time",
			precision: e?.precision === void 0 ? null : e?.precision,
			...b.errToObj(e?.message)
		});
	}
	duration(e) {
		return this._addCheck({
			kind: "duration",
			...b.errToObj(e)
		});
	}
	regex(e, t) {
		return this._addCheck({
			kind: "regex",
			regex: e,
			...b.errToObj(t)
		});
	}
	includes(e, t) {
		return this._addCheck({
			kind: "includes",
			value: e,
			position: t?.position,
			...b.errToObj(t?.message)
		});
	}
	startsWith(e, t) {
		return this._addCheck({
			kind: "startsWith",
			value: e,
			...b.errToObj(t)
		});
	}
	endsWith(e, t) {
		return this._addCheck({
			kind: "endsWith",
			value: e,
			...b.errToObj(t)
		});
	}
	min(e, t) {
		return this._addCheck({
			kind: "min",
			value: e,
			...b.errToObj(t)
		});
	}
	max(e, t) {
		return this._addCheck({
			kind: "max",
			value: e,
			...b.errToObj(t)
		});
	}
	length(e, t) {
		return this._addCheck({
			kind: "length",
			value: e,
			...b.errToObj(t)
		});
	}
	nonempty(e) {
		return this.min(1, b.errToObj(e));
	}
	trim() {
		return new e({
			...this._def,
			checks: [...this._def.checks, { kind: "trim" }]
		});
	}
	toLowerCase() {
		return new e({
			...this._def,
			checks: [...this._def.checks, { kind: "toLowerCase" }]
		});
	}
	toUpperCase() {
		return new e({
			...this._def,
			checks: [...this._def.checks, { kind: "toUpperCase" }]
		});
	}
	get isDatetime() {
		return !!this._def.checks.find((e) => e.kind === "datetime");
	}
	get isDate() {
		return !!this._def.checks.find((e) => e.kind === "date");
	}
	get isTime() {
		return !!this._def.checks.find((e) => e.kind === "time");
	}
	get isDuration() {
		return !!this._def.checks.find((e) => e.kind === "duration");
	}
	get isEmail() {
		return !!this._def.checks.find((e) => e.kind === "email");
	}
	get isURL() {
		return !!this._def.checks.find((e) => e.kind === "url");
	}
	get isEmoji() {
		return !!this._def.checks.find((e) => e.kind === "emoji");
	}
	get isUUID() {
		return !!this._def.checks.find((e) => e.kind === "uuid");
	}
	get isNANOID() {
		return !!this._def.checks.find((e) => e.kind === "nanoid");
	}
	get isCUID() {
		return !!this._def.checks.find((e) => e.kind === "cuid");
	}
	get isCUID2() {
		return !!this._def.checks.find((e) => e.kind === "cuid2");
	}
	get isULID() {
		return !!this._def.checks.find((e) => e.kind === "ulid");
	}
	get isIP() {
		return !!this._def.checks.find((e) => e.kind === "ip");
	}
	get isCIDR() {
		return !!this._def.checks.find((e) => e.kind === "cidr");
	}
	get isBase64() {
		return !!this._def.checks.find((e) => e.kind === "base64");
	}
	get isBase64url() {
		return !!this._def.checks.find((e) => e.kind === "base64url");
	}
	get minLength() {
		let e = null;
		for (let t of this._def.checks) t.kind === "min" && (e === null || t.value > e) && (e = t.value);
		return e;
	}
	get maxLength() {
		let e = null;
		for (let t of this._def.checks) t.kind === "max" && (e === null || t.value < e) && (e = t.value);
		return e;
	}
};
Ne.create = (e) => new Ne({
	checks: [],
	typeName: D.ZodString,
	coerce: e?.coerce ?? !1,
	...S(e)
});
function Pe(e, t) {
	let n = (e.toString().split(".")[1] || "").length, r = (t.toString().split(".")[1] || "").length, i = n > r ? n : r;
	return Number.parseInt(e.toFixed(i).replace(".", "")) % Number.parseInt(t.toFixed(i).replace(".", "")) / 10 ** i;
}
var Fe = class e extends C {
	constructor() {
		super(...arguments), this.min = this.gte, this.max = this.lte, this.step = this.multipleOf;
	}
	_parse(e) {
		if (this._def.coerce && (e.data = Number(e.data)), this._getType(e) !== d.number) {
			let t = this._getOrReturnCtx(e);
			return g(t, {
				code: p.invalid_type,
				expected: d.number,
				received: t.parsedType
			}), v;
		}
		let t, n = new _();
		for (let r of this._def.checks) r.kind === "int" ? l.isInteger(e.data) || (t = this._getOrReturnCtx(e, t), g(t, {
			code: p.invalid_type,
			expected: "integer",
			received: "float",
			message: r.message
		}), n.dirty()) : r.kind === "min" ? (r.inclusive ? e.data < r.value : e.data <= r.value) && (t = this._getOrReturnCtx(e, t), g(t, {
			code: p.too_small,
			minimum: r.value,
			type: "number",
			inclusive: r.inclusive,
			exact: !1,
			message: r.message
		}), n.dirty()) : r.kind === "max" ? (r.inclusive ? e.data > r.value : e.data >= r.value) && (t = this._getOrReturnCtx(e, t), g(t, {
			code: p.too_big,
			maximum: r.value,
			type: "number",
			inclusive: r.inclusive,
			exact: !1,
			message: r.message
		}), n.dirty()) : r.kind === "multipleOf" ? Pe(e.data, r.value) !== 0 && (t = this._getOrReturnCtx(e, t), g(t, {
			code: p.not_multiple_of,
			multipleOf: r.value,
			message: r.message
		}), n.dirty()) : r.kind === "finite" ? Number.isFinite(e.data) || (t = this._getOrReturnCtx(e, t), g(t, {
			code: p.not_finite,
			message: r.message
		}), n.dirty()) : l.assertNever(r);
		return {
			status: n.value,
			value: e.data
		};
	}
	gte(e, t) {
		return this.setLimit("min", e, !0, b.toString(t));
	}
	gt(e, t) {
		return this.setLimit("min", e, !1, b.toString(t));
	}
	lte(e, t) {
		return this.setLimit("max", e, !0, b.toString(t));
	}
	lt(e, t) {
		return this.setLimit("max", e, !1, b.toString(t));
	}
	setLimit(t, n, r, i) {
		return new e({
			...this._def,
			checks: [...this._def.checks, {
				kind: t,
				value: n,
				inclusive: r,
				message: b.toString(i)
			}]
		});
	}
	_addCheck(t) {
		return new e({
			...this._def,
			checks: [...this._def.checks, t]
		});
	}
	int(e) {
		return this._addCheck({
			kind: "int",
			message: b.toString(e)
		});
	}
	positive(e) {
		return this._addCheck({
			kind: "min",
			value: 0,
			inclusive: !1,
			message: b.toString(e)
		});
	}
	negative(e) {
		return this._addCheck({
			kind: "max",
			value: 0,
			inclusive: !1,
			message: b.toString(e)
		});
	}
	nonpositive(e) {
		return this._addCheck({
			kind: "max",
			value: 0,
			inclusive: !0,
			message: b.toString(e)
		});
	}
	nonnegative(e) {
		return this._addCheck({
			kind: "min",
			value: 0,
			inclusive: !0,
			message: b.toString(e)
		});
	}
	multipleOf(e, t) {
		return this._addCheck({
			kind: "multipleOf",
			value: e,
			message: b.toString(t)
		});
	}
	finite(e) {
		return this._addCheck({
			kind: "finite",
			message: b.toString(e)
		});
	}
	safe(e) {
		return this._addCheck({
			kind: "min",
			inclusive: !0,
			value: -(2 ** 53 - 1),
			message: b.toString(e)
		})._addCheck({
			kind: "max",
			inclusive: !0,
			value: 2 ** 53 - 1,
			message: b.toString(e)
		});
	}
	get minValue() {
		let e = null;
		for (let t of this._def.checks) t.kind === "min" && (e === null || t.value > e) && (e = t.value);
		return e;
	}
	get maxValue() {
		let e = null;
		for (let t of this._def.checks) t.kind === "max" && (e === null || t.value < e) && (e = t.value);
		return e;
	}
	get isInt() {
		return !!this._def.checks.find((e) => e.kind === "int" || e.kind === "multipleOf" && l.isInteger(e.value));
	}
	get isFinite() {
		let e = null, t = null;
		for (let n of this._def.checks) if (n.kind === "finite" || n.kind === "int" || n.kind === "multipleOf") return !0;
		else n.kind === "min" ? (t === null || n.value > t) && (t = n.value) : n.kind === "max" && (e === null || n.value < e) && (e = n.value);
		return Number.isFinite(t) && Number.isFinite(e);
	}
};
Fe.create = (e) => new Fe({
	checks: [],
	typeName: D.ZodNumber,
	coerce: e?.coerce || !1,
	...S(e)
});
var Ie = class e extends C {
	constructor() {
		super(...arguments), this.min = this.gte, this.max = this.lte;
	}
	_parse(e) {
		if (this._def.coerce) try {
			e.data = BigInt(e.data);
		} catch {
			return this._getInvalidInput(e);
		}
		if (this._getType(e) !== d.bigint) return this._getInvalidInput(e);
		let t, n = new _();
		for (let r of this._def.checks) r.kind === "min" ? (r.inclusive ? e.data < r.value : e.data <= r.value) && (t = this._getOrReturnCtx(e, t), g(t, {
			code: p.too_small,
			type: "bigint",
			minimum: r.value,
			inclusive: r.inclusive,
			message: r.message
		}), n.dirty()) : r.kind === "max" ? (r.inclusive ? e.data > r.value : e.data >= r.value) && (t = this._getOrReturnCtx(e, t), g(t, {
			code: p.too_big,
			type: "bigint",
			maximum: r.value,
			inclusive: r.inclusive,
			message: r.message
		}), n.dirty()) : r.kind === "multipleOf" ? e.data % r.value !== BigInt(0) && (t = this._getOrReturnCtx(e, t), g(t, {
			code: p.not_multiple_of,
			multipleOf: r.value,
			message: r.message
		}), n.dirty()) : l.assertNever(r);
		return {
			status: n.value,
			value: e.data
		};
	}
	_getInvalidInput(e) {
		let t = this._getOrReturnCtx(e);
		return g(t, {
			code: p.invalid_type,
			expected: d.bigint,
			received: t.parsedType
		}), v;
	}
	gte(e, t) {
		return this.setLimit("min", e, !0, b.toString(t));
	}
	gt(e, t) {
		return this.setLimit("min", e, !1, b.toString(t));
	}
	lte(e, t) {
		return this.setLimit("max", e, !0, b.toString(t));
	}
	lt(e, t) {
		return this.setLimit("max", e, !1, b.toString(t));
	}
	setLimit(t, n, r, i) {
		return new e({
			...this._def,
			checks: [...this._def.checks, {
				kind: t,
				value: n,
				inclusive: r,
				message: b.toString(i)
			}]
		});
	}
	_addCheck(t) {
		return new e({
			...this._def,
			checks: [...this._def.checks, t]
		});
	}
	positive(e) {
		return this._addCheck({
			kind: "min",
			value: BigInt(0),
			inclusive: !1,
			message: b.toString(e)
		});
	}
	negative(e) {
		return this._addCheck({
			kind: "max",
			value: BigInt(0),
			inclusive: !1,
			message: b.toString(e)
		});
	}
	nonpositive(e) {
		return this._addCheck({
			kind: "max",
			value: BigInt(0),
			inclusive: !0,
			message: b.toString(e)
		});
	}
	nonnegative(e) {
		return this._addCheck({
			kind: "min",
			value: BigInt(0),
			inclusive: !0,
			message: b.toString(e)
		});
	}
	multipleOf(e, t) {
		return this._addCheck({
			kind: "multipleOf",
			value: e,
			message: b.toString(t)
		});
	}
	get minValue() {
		let e = null;
		for (let t of this._def.checks) t.kind === "min" && (e === null || t.value > e) && (e = t.value);
		return e;
	}
	get maxValue() {
		let e = null;
		for (let t of this._def.checks) t.kind === "max" && (e === null || t.value < e) && (e = t.value);
		return e;
	}
};
Ie.create = (e) => new Ie({
	checks: [],
	typeName: D.ZodBigInt,
	coerce: e?.coerce ?? !1,
	...S(e)
});
var Le = class extends C {
	_parse(e) {
		if (this._def.coerce && (e.data = !!e.data), this._getType(e) !== d.boolean) {
			let t = this._getOrReturnCtx(e);
			return g(t, {
				code: p.invalid_type,
				expected: d.boolean,
				received: t.parsedType
			}), v;
		}
		return y(e.data);
	}
};
Le.create = (e) => new Le({
	typeName: D.ZodBoolean,
	coerce: e?.coerce || !1,
	...S(e)
});
var Re = class e extends C {
	_parse(e) {
		if (this._def.coerce && (e.data = new Date(e.data)), this._getType(e) !== d.date) {
			let t = this._getOrReturnCtx(e);
			return g(t, {
				code: p.invalid_type,
				expected: d.date,
				received: t.parsedType
			}), v;
		}
		if (Number.isNaN(e.data.getTime())) return g(this._getOrReturnCtx(e), { code: p.invalid_date }), v;
		let t = new _(), n;
		for (let r of this._def.checks) r.kind === "min" ? e.data.getTime() < r.value && (n = this._getOrReturnCtx(e, n), g(n, {
			code: p.too_small,
			message: r.message,
			inclusive: !0,
			exact: !1,
			minimum: r.value,
			type: "date"
		}), t.dirty()) : r.kind === "max" ? e.data.getTime() > r.value && (n = this._getOrReturnCtx(e, n), g(n, {
			code: p.too_big,
			message: r.message,
			inclusive: !0,
			exact: !1,
			maximum: r.value,
			type: "date"
		}), t.dirty()) : l.assertNever(r);
		return {
			status: t.value,
			value: new Date(e.data.getTime())
		};
	}
	_addCheck(t) {
		return new e({
			...this._def,
			checks: [...this._def.checks, t]
		});
	}
	min(e, t) {
		return this._addCheck({
			kind: "min",
			value: e.getTime(),
			message: b.toString(t)
		});
	}
	max(e, t) {
		return this._addCheck({
			kind: "max",
			value: e.getTime(),
			message: b.toString(t)
		});
	}
	get minDate() {
		let e = null;
		for (let t of this._def.checks) t.kind === "min" && (e === null || t.value > e) && (e = t.value);
		return e == null ? null : new Date(e);
	}
	get maxDate() {
		let e = null;
		for (let t of this._def.checks) t.kind === "max" && (e === null || t.value < e) && (e = t.value);
		return e == null ? null : new Date(e);
	}
};
Re.create = (e) => new Re({
	checks: [],
	coerce: e?.coerce || !1,
	typeName: D.ZodDate,
	...S(e)
});
var ze = class extends C {
	_parse(e) {
		if (this._getType(e) !== d.symbol) {
			let t = this._getOrReturnCtx(e);
			return g(t, {
				code: p.invalid_type,
				expected: d.symbol,
				received: t.parsedType
			}), v;
		}
		return y(e.data);
	}
};
ze.create = (e) => new ze({
	typeName: D.ZodSymbol,
	...S(e)
});
var Be = class extends C {
	_parse(e) {
		if (this._getType(e) !== d.undefined) {
			let t = this._getOrReturnCtx(e);
			return g(t, {
				code: p.invalid_type,
				expected: d.undefined,
				received: t.parsedType
			}), v;
		}
		return y(e.data);
	}
};
Be.create = (e) => new Be({
	typeName: D.ZodUndefined,
	...S(e)
});
var Ve = class extends C {
	_parse(e) {
		if (this._getType(e) !== d.null) {
			let t = this._getOrReturnCtx(e);
			return g(t, {
				code: p.invalid_type,
				expected: d.null,
				received: t.parsedType
			}), v;
		}
		return y(e.data);
	}
};
Ve.create = (e) => new Ve({
	typeName: D.ZodNull,
	...S(e)
});
var He = class extends C {
	constructor() {
		super(...arguments), this._any = !0;
	}
	_parse(e) {
		return y(e.data);
	}
};
He.create = (e) => new He({
	typeName: D.ZodAny,
	...S(e)
});
var Ue = class extends C {
	constructor() {
		super(...arguments), this._unknown = !0;
	}
	_parse(e) {
		return y(e.data);
	}
};
Ue.create = (e) => new Ue({
	typeName: D.ZodUnknown,
	...S(e)
});
var We = class extends C {
	_parse(e) {
		let t = this._getOrReturnCtx(e);
		return g(t, {
			code: p.invalid_type,
			expected: d.never,
			received: t.parsedType
		}), v;
	}
};
We.create = (e) => new We({
	typeName: D.ZodNever,
	...S(e)
});
var Ge = class extends C {
	_parse(e) {
		if (this._getType(e) !== d.undefined) {
			let t = this._getOrReturnCtx(e);
			return g(t, {
				code: p.invalid_type,
				expected: d.void,
				received: t.parsedType
			}), v;
		}
		return y(e.data);
	}
};
Ge.create = (e) => new Ge({
	typeName: D.ZodVoid,
	...S(e)
});
var Ke = class e extends C {
	_parse(e) {
		let { ctx: t, status: n } = this._processInputParams(e), r = this._def;
		if (t.parsedType !== d.array) return g(t, {
			code: p.invalid_type,
			expected: d.array,
			received: t.parsedType
		}), v;
		if (r.exactLength !== null) {
			let e = t.data.length > r.exactLength.value, i = t.data.length < r.exactLength.value;
			(e || i) && (g(t, {
				code: e ? p.too_big : p.too_small,
				minimum: i ? r.exactLength.value : void 0,
				maximum: e ? r.exactLength.value : void 0,
				type: "array",
				inclusive: !0,
				exact: !0,
				message: r.exactLength.message
			}), n.dirty());
		}
		if (r.minLength !== null && t.data.length < r.minLength.value && (g(t, {
			code: p.too_small,
			minimum: r.minLength.value,
			type: "array",
			inclusive: !0,
			exact: !1,
			message: r.minLength.message
		}), n.dirty()), r.maxLength !== null && t.data.length > r.maxLength.value && (g(t, {
			code: p.too_big,
			maximum: r.maxLength.value,
			type: "array",
			inclusive: !0,
			exact: !1,
			message: r.maxLength.message
		}), n.dirty()), t.common.async) return Promise.all([...t.data].map((e, n) => r.type._parseAsync(new x(t, e, t.path, n)))).then((e) => _.mergeArray(n, e));
		let i = [...t.data].map((e, n) => r.type._parseSync(new x(t, e, t.path, n)));
		return _.mergeArray(n, i);
	}
	get element() {
		return this._def.type;
	}
	min(t, n) {
		return new e({
			...this._def,
			minLength: {
				value: t,
				message: b.toString(n)
			}
		});
	}
	max(t, n) {
		return new e({
			...this._def,
			maxLength: {
				value: t,
				message: b.toString(n)
			}
		});
	}
	length(t, n) {
		return new e({
			...this._def,
			exactLength: {
				value: t,
				message: b.toString(n)
			}
		});
	}
	nonempty(e) {
		return this.min(1, e);
	}
};
Ke.create = (e, t) => new Ke({
	type: e,
	minLength: null,
	maxLength: null,
	exactLength: null,
	typeName: D.ZodArray,
	...S(t)
});
function qe(e) {
	if (e instanceof w) {
		let t = {};
		for (let n in e.shape) {
			let r = e.shape[n];
			t[n] = E.create(qe(r));
		}
		return new w({
			...e._def,
			shape: () => t
		});
	} else if (e instanceof Ke) return new Ke({
		...e._def,
		type: qe(e.element)
	});
	else if (e instanceof E) return E.create(qe(e.unwrap()));
	else if (e instanceof ut) return ut.create(qe(e.unwrap()));
	else if (e instanceof $e) return $e.create(e.items.map((e) => qe(e)));
	else return e;
}
var w = class e extends C {
	constructor() {
		super(...arguments), this._cached = null, this.nonstrict = this.passthrough, this.augment = this.extend;
	}
	_getCached() {
		if (this._cached !== null) return this._cached;
		let e = this._def.shape(), t = l.objectKeys(e);
		return this._cached = {
			shape: e,
			keys: t
		}, this._cached;
	}
	_parse(e) {
		if (this._getType(e) !== d.object) {
			let t = this._getOrReturnCtx(e);
			return g(t, {
				code: p.invalid_type,
				expected: d.object,
				received: t.parsedType
			}), v;
		}
		let { status: t, ctx: n } = this._processInputParams(e), { shape: r, keys: i } = this._getCached(), a = [];
		if (!(this._def.catchall instanceof We && this._def.unknownKeys === "strip")) for (let e in n.data) i.includes(e) || a.push(e);
		let o = [];
		for (let e of i) {
			let t = r[e], i = n.data[e];
			o.push({
				key: {
					status: "valid",
					value: e
				},
				value: t._parse(new x(n, i, n.path, e)),
				alwaysSet: e in n.data
			});
		}
		if (this._def.catchall instanceof We) {
			let e = this._def.unknownKeys;
			if (e === "passthrough") for (let e of a) o.push({
				key: {
					status: "valid",
					value: e
				},
				value: {
					status: "valid",
					value: n.data[e]
				}
			});
			else if (e === "strict") a.length > 0 && (g(n, {
				code: p.unrecognized_keys,
				keys: a
			}), t.dirty());
			else if (e !== "strip") throw Error("Internal ZodObject error: invalid unknownKeys value.");
		} else {
			let e = this._def.catchall;
			for (let t of a) {
				let r = n.data[t];
				o.push({
					key: {
						status: "valid",
						value: t
					},
					value: e._parse(new x(n, r, n.path, t)),
					alwaysSet: t in n.data
				});
			}
		}
		return n.common.async ? Promise.resolve().then(async () => {
			let e = [];
			for (let t of o) {
				let n = await t.key, r = await t.value;
				e.push({
					key: n,
					value: r,
					alwaysSet: t.alwaysSet
				});
			}
			return e;
		}).then((e) => _.mergeObjectSync(t, e)) : _.mergeObjectSync(t, o);
	}
	get shape() {
		return this._def.shape();
	}
	strict(t) {
		return b.errToObj, new e({
			...this._def,
			unknownKeys: "strict",
			...t === void 0 ? {} : { errorMap: (e, n) => {
				let r = this._def.errorMap?.(e, n).message ?? n.defaultError;
				return e.code === "unrecognized_keys" ? { message: b.errToObj(t).message ?? r } : { message: r };
			} }
		});
	}
	strip() {
		return new e({
			...this._def,
			unknownKeys: "strip"
		});
	}
	passthrough() {
		return new e({
			...this._def,
			unknownKeys: "passthrough"
		});
	}
	extend(t) {
		return new e({
			...this._def,
			shape: () => ({
				...this._def.shape(),
				...t
			})
		});
	}
	merge(t) {
		return new e({
			unknownKeys: t._def.unknownKeys,
			catchall: t._def.catchall,
			shape: () => ({
				...this._def.shape(),
				...t._def.shape()
			}),
			typeName: D.ZodObject
		});
	}
	setKey(e, t) {
		return this.augment({ [e]: t });
	}
	catchall(t) {
		return new e({
			...this._def,
			catchall: t
		});
	}
	pick(t) {
		let n = {};
		for (let e of l.objectKeys(t)) t[e] && this.shape[e] && (n[e] = this.shape[e]);
		return new e({
			...this._def,
			shape: () => n
		});
	}
	omit(t) {
		let n = {};
		for (let e of l.objectKeys(this.shape)) t[e] || (n[e] = this.shape[e]);
		return new e({
			...this._def,
			shape: () => n
		});
	}
	deepPartial() {
		return qe(this);
	}
	partial(t) {
		let n = {};
		for (let e of l.objectKeys(this.shape)) {
			let r = this.shape[e];
			t && !t[e] ? n[e] = r : n[e] = r.optional();
		}
		return new e({
			...this._def,
			shape: () => n
		});
	}
	required(t) {
		let n = {};
		for (let e of l.objectKeys(this.shape)) if (t && !t[e]) n[e] = this.shape[e];
		else {
			let t = this.shape[e];
			for (; t instanceof E;) t = t._def.innerType;
			n[e] = t;
		}
		return new e({
			...this._def,
			shape: () => n
		});
	}
	keyof() {
		return ot(l.objectKeys(this.shape));
	}
};
w.create = (e, t) => new w({
	shape: () => e,
	unknownKeys: "strip",
	catchall: We.create(),
	typeName: D.ZodObject,
	...S(t)
}), w.strictCreate = (e, t) => new w({
	shape: () => e,
	unknownKeys: "strict",
	catchall: We.create(),
	typeName: D.ZodObject,
	...S(t)
}), w.lazycreate = (e, t) => new w({
	shape: e,
	unknownKeys: "strip",
	catchall: We.create(),
	typeName: D.ZodObject,
	...S(t)
});
var Je = class extends C {
	_parse(e) {
		let { ctx: t } = this._processInputParams(e), n = this._def.options;
		function r(e) {
			for (let t of e) if (t.result.status === "valid") return t.result;
			for (let n of e) if (n.result.status === "dirty") return t.common.issues.push(...n.ctx.common.issues), n.result;
			let n = e.map((e) => new m(e.ctx.common.issues));
			return g(t, {
				code: p.invalid_union,
				unionErrors: n
			}), v;
		}
		if (t.common.async) return Promise.all(n.map(async (e) => {
			let n = {
				...t,
				common: {
					...t.common,
					issues: []
				},
				parent: null
			};
			return {
				result: await e._parseAsync({
					data: t.data,
					path: t.path,
					parent: n
				}),
				ctx: n
			};
		})).then(r);
		{
			let e, r = [];
			for (let i of n) {
				let n = {
					...t,
					common: {
						...t.common,
						issues: []
					},
					parent: null
				}, a = i._parseSync({
					data: t.data,
					path: t.path,
					parent: n
				});
				if (a.status === "valid") return a;
				a.status === "dirty" && !e && (e = {
					result: a,
					ctx: n
				}), n.common.issues.length && r.push(n.common.issues);
			}
			if (e) return t.common.issues.push(...e.ctx.common.issues), e.result;
			let i = r.map((e) => new m(e));
			return g(t, {
				code: p.invalid_union,
				unionErrors: i
			}), v;
		}
	}
	get options() {
		return this._def.options;
	}
};
Je.create = (e, t) => new Je({
	options: e,
	typeName: D.ZodUnion,
	...S(t)
});
var Ye = (e) => e instanceof it ? Ye(e.schema) : e instanceof T ? Ye(e.innerType()) : e instanceof at ? [e.value] : e instanceof st ? e.options : e instanceof ct ? l.objectValues(e.enum) : e instanceof dt ? Ye(e._def.innerType) : e instanceof Be ? [void 0] : e instanceof Ve ? [null] : e instanceof E ? [void 0, ...Ye(e.unwrap())] : e instanceof ut ? [null, ...Ye(e.unwrap())] : e instanceof mt || e instanceof gt ? Ye(e.unwrap()) : e instanceof ft ? Ye(e._def.innerType) : [], Xe = class e extends C {
	_parse(e) {
		let { ctx: t } = this._processInputParams(e);
		if (t.parsedType !== d.object) return g(t, {
			code: p.invalid_type,
			expected: d.object,
			received: t.parsedType
		}), v;
		let n = this.discriminator, r = t.data[n], i = this.optionsMap.get(r);
		return i ? t.common.async ? i._parseAsync({
			data: t.data,
			path: t.path,
			parent: t
		}) : i._parseSync({
			data: t.data,
			path: t.path,
			parent: t
		}) : (g(t, {
			code: p.invalid_union_discriminator,
			options: Array.from(this.optionsMap.keys()),
			path: [n]
		}), v);
	}
	get discriminator() {
		return this._def.discriminator;
	}
	get options() {
		return this._def.options;
	}
	get optionsMap() {
		return this._def.optionsMap;
	}
	static create(t, n, r) {
		let i = /* @__PURE__ */ new Map();
		for (let e of n) {
			let n = Ye(e.shape[t]);
			if (!n.length) throw Error(`A discriminator value for key \`${t}\` could not be extracted from all schema options`);
			for (let r of n) {
				if (i.has(r)) throw Error(`Discriminator property ${String(t)} has duplicate value ${String(r)}`);
				i.set(r, e);
			}
		}
		return new e({
			typeName: D.ZodDiscriminatedUnion,
			discriminator: t,
			options: n,
			optionsMap: i,
			...S(r)
		});
	}
};
function Ze(e, t) {
	let n = f(e), r = f(t);
	if (e === t) return {
		valid: !0,
		data: e
	};
	if (n === d.object && r === d.object) {
		let n = l.objectKeys(t), r = l.objectKeys(e).filter((e) => n.indexOf(e) !== -1), i = {
			...e,
			...t
		};
		for (let n of r) {
			let r = Ze(e[n], t[n]);
			if (!r.valid) return { valid: !1 };
			i[n] = r.data;
		}
		return {
			valid: !0,
			data: i
		};
	} else if (n === d.array && r === d.array) {
		if (e.length !== t.length) return { valid: !1 };
		let n = [];
		for (let r = 0; r < e.length; r++) {
			let i = e[r], a = t[r], o = Ze(i, a);
			if (!o.valid) return { valid: !1 };
			n.push(o.data);
		}
		return {
			valid: !0,
			data: n
		};
	} else if (n === d.date && r === d.date && +e == +t) return {
		valid: !0,
		data: e
	};
	else return { valid: !1 };
}
var Qe = class extends C {
	_parse(e) {
		let { status: t, ctx: n } = this._processInputParams(e), r = (e, r) => {
			if (ie(e) || ie(r)) return v;
			let i = Ze(e.value, r.value);
			return i.valid ? ((ae(e) || ae(r)) && t.dirty(), {
				status: t.value,
				value: i.data
			}) : (g(n, { code: p.invalid_intersection_types }), v);
		};
		return n.common.async ? Promise.all([this._def.left._parseAsync({
			data: n.data,
			path: n.path,
			parent: n
		}), this._def.right._parseAsync({
			data: n.data,
			path: n.path,
			parent: n
		})]).then(([e, t]) => r(e, t)) : r(this._def.left._parseSync({
			data: n.data,
			path: n.path,
			parent: n
		}), this._def.right._parseSync({
			data: n.data,
			path: n.path,
			parent: n
		}));
	}
};
Qe.create = (e, t, n) => new Qe({
	left: e,
	right: t,
	typeName: D.ZodIntersection,
	...S(n)
});
var $e = class e extends C {
	_parse(e) {
		let { status: t, ctx: n } = this._processInputParams(e);
		if (n.parsedType !== d.array) return g(n, {
			code: p.invalid_type,
			expected: d.array,
			received: n.parsedType
		}), v;
		if (n.data.length < this._def.items.length) return g(n, {
			code: p.too_small,
			minimum: this._def.items.length,
			inclusive: !0,
			exact: !1,
			type: "array"
		}), v;
		!this._def.rest && n.data.length > this._def.items.length && (g(n, {
			code: p.too_big,
			maximum: this._def.items.length,
			inclusive: !0,
			exact: !1,
			type: "array"
		}), t.dirty());
		let r = [...n.data].map((e, t) => {
			let r = this._def.items[t] || this._def.rest;
			return r ? r._parse(new x(n, e, n.path, t)) : null;
		}).filter((e) => !!e);
		return n.common.async ? Promise.all(r).then((e) => _.mergeArray(t, e)) : _.mergeArray(t, r);
	}
	get items() {
		return this._def.items;
	}
	rest(t) {
		return new e({
			...this._def,
			rest: t
		});
	}
};
$e.create = (e, t) => {
	if (!Array.isArray(e)) throw Error("You must pass an array of schemas to z.tuple([ ... ])");
	return new $e({
		items: e,
		typeName: D.ZodTuple,
		rest: null,
		...S(t)
	});
};
var et = class e extends C {
	get keySchema() {
		return this._def.keyType;
	}
	get valueSchema() {
		return this._def.valueType;
	}
	_parse(e) {
		let { status: t, ctx: n } = this._processInputParams(e);
		if (n.parsedType !== d.object) return g(n, {
			code: p.invalid_type,
			expected: d.object,
			received: n.parsedType
		}), v;
		let r = [], i = this._def.keyType, a = this._def.valueType;
		for (let e in n.data) r.push({
			key: i._parse(new x(n, e, n.path, e)),
			value: a._parse(new x(n, n.data[e], n.path, e)),
			alwaysSet: e in n.data
		});
		return n.common.async ? _.mergeObjectAsync(t, r) : _.mergeObjectSync(t, r);
	}
	get element() {
		return this._def.valueType;
	}
	static create(t, n, r) {
		return n instanceof C ? new e({
			keyType: t,
			valueType: n,
			typeName: D.ZodRecord,
			...S(r)
		}) : new e({
			keyType: Ne.create(),
			valueType: t,
			typeName: D.ZodRecord,
			...S(n)
		});
	}
}, tt = class extends C {
	get keySchema() {
		return this._def.keyType;
	}
	get valueSchema() {
		return this._def.valueType;
	}
	_parse(e) {
		let { status: t, ctx: n } = this._processInputParams(e);
		if (n.parsedType !== d.map) return g(n, {
			code: p.invalid_type,
			expected: d.map,
			received: n.parsedType
		}), v;
		let r = this._def.keyType, i = this._def.valueType, a = [...n.data.entries()].map(([e, t], a) => ({
			key: r._parse(new x(n, e, n.path, [a, "key"])),
			value: i._parse(new x(n, t, n.path, [a, "value"]))
		}));
		if (n.common.async) {
			let e = /* @__PURE__ */ new Map();
			return Promise.resolve().then(async () => {
				for (let n of a) {
					let r = await n.key, i = await n.value;
					if (r.status === "aborted" || i.status === "aborted") return v;
					(r.status === "dirty" || i.status === "dirty") && t.dirty(), e.set(r.value, i.value);
				}
				return {
					status: t.value,
					value: e
				};
			});
		} else {
			let e = /* @__PURE__ */ new Map();
			for (let n of a) {
				let r = n.key, i = n.value;
				if (r.status === "aborted" || i.status === "aborted") return v;
				(r.status === "dirty" || i.status === "dirty") && t.dirty(), e.set(r.value, i.value);
			}
			return {
				status: t.value,
				value: e
			};
		}
	}
};
tt.create = (e, t, n) => new tt({
	valueType: t,
	keyType: e,
	typeName: D.ZodMap,
	...S(n)
});
var nt = class e extends C {
	_parse(e) {
		let { status: t, ctx: n } = this._processInputParams(e);
		if (n.parsedType !== d.set) return g(n, {
			code: p.invalid_type,
			expected: d.set,
			received: n.parsedType
		}), v;
		let r = this._def;
		r.minSize !== null && n.data.size < r.minSize.value && (g(n, {
			code: p.too_small,
			minimum: r.minSize.value,
			type: "set",
			inclusive: !0,
			exact: !1,
			message: r.minSize.message
		}), t.dirty()), r.maxSize !== null && n.data.size > r.maxSize.value && (g(n, {
			code: p.too_big,
			maximum: r.maxSize.value,
			type: "set",
			inclusive: !0,
			exact: !1,
			message: r.maxSize.message
		}), t.dirty());
		let i = this._def.valueType;
		function a(e) {
			let n = /* @__PURE__ */ new Set();
			for (let r of e) {
				if (r.status === "aborted") return v;
				r.status === "dirty" && t.dirty(), n.add(r.value);
			}
			return {
				status: t.value,
				value: n
			};
		}
		let o = [...n.data.values()].map((e, t) => i._parse(new x(n, e, n.path, t)));
		return n.common.async ? Promise.all(o).then((e) => a(e)) : a(o);
	}
	min(t, n) {
		return new e({
			...this._def,
			minSize: {
				value: t,
				message: b.toString(n)
			}
		});
	}
	max(t, n) {
		return new e({
			...this._def,
			maxSize: {
				value: t,
				message: b.toString(n)
			}
		});
	}
	size(e, t) {
		return this.min(e, t).max(e, t);
	}
	nonempty(e) {
		return this.min(1, e);
	}
};
nt.create = (e, t) => new nt({
	valueType: e,
	minSize: null,
	maxSize: null,
	typeName: D.ZodSet,
	...S(t)
});
var rt = class e extends C {
	constructor() {
		super(...arguments), this.validate = this.implement;
	}
	_parse(e) {
		let { ctx: t } = this._processInputParams(e);
		if (t.parsedType !== d.function) return g(t, {
			code: p.invalid_type,
			expected: d.function,
			received: t.parsedType
		}), v;
		function n(e, n) {
			return ne({
				data: e,
				path: t.path,
				errorMaps: [
					t.common.contextualErrorMap,
					t.schemaErrorMap,
					te(),
					h
				].filter((e) => !!e),
				issueData: {
					code: p.invalid_arguments,
					argumentsError: n
				}
			});
		}
		function r(e, n) {
			return ne({
				data: e,
				path: t.path,
				errorMaps: [
					t.common.contextualErrorMap,
					t.schemaErrorMap,
					te(),
					h
				].filter((e) => !!e),
				issueData: {
					code: p.invalid_return_type,
					returnTypeError: n
				}
			});
		}
		let i = { errorMap: t.common.contextualErrorMap }, a = t.data;
		if (this._def.returns instanceof lt) {
			let e = this;
			return y(async function(...t) {
				let o = new m([]), s = await e._def.args.parseAsync(t, i).catch((e) => {
					throw o.addIssue(n(t, e)), o;
				}), c = await Reflect.apply(a, this, s);
				return await e._def.returns._def.type.parseAsync(c, i).catch((e) => {
					throw o.addIssue(r(c, e)), o;
				});
			});
		} else {
			let e = this;
			return y(function(...t) {
				let o = e._def.args.safeParse(t, i);
				if (!o.success) throw new m([n(t, o.error)]);
				let s = Reflect.apply(a, this, o.data), c = e._def.returns.safeParse(s, i);
				if (!c.success) throw new m([r(s, c.error)]);
				return c.data;
			});
		}
	}
	parameters() {
		return this._def.args;
	}
	returnType() {
		return this._def.returns;
	}
	args(...t) {
		return new e({
			...this._def,
			args: $e.create(t).rest(Ue.create())
		});
	}
	returns(t) {
		return new e({
			...this._def,
			returns: t
		});
	}
	implement(e) {
		return this.parse(e);
	}
	strictImplement(e) {
		return this.parse(e);
	}
	static create(t, n, r) {
		return new e({
			args: t || $e.create([]).rest(Ue.create()),
			returns: n || Ue.create(),
			typeName: D.ZodFunction,
			...S(r)
		});
	}
}, it = class extends C {
	get schema() {
		return this._def.getter();
	}
	_parse(e) {
		let { ctx: t } = this._processInputParams(e);
		return this._def.getter()._parse({
			data: t.data,
			path: t.path,
			parent: t
		});
	}
};
it.create = (e, t) => new it({
	getter: e,
	typeName: D.ZodLazy,
	...S(t)
});
var at = class extends C {
	_parse(e) {
		if (e.data !== this._def.value) {
			let t = this._getOrReturnCtx(e);
			return g(t, {
				received: t.data,
				code: p.invalid_literal,
				expected: this._def.value
			}), v;
		}
		return {
			status: "valid",
			value: e.data
		};
	}
	get value() {
		return this._def.value;
	}
};
at.create = (e, t) => new at({
	value: e,
	typeName: D.ZodLiteral,
	...S(t)
});
function ot(e, t) {
	return new st({
		values: e,
		typeName: D.ZodEnum,
		...S(t)
	});
}
var st = class e extends C {
	_parse(e) {
		if (typeof e.data != "string") {
			let t = this._getOrReturnCtx(e), n = this._def.values;
			return g(t, {
				expected: l.joinValues(n),
				received: t.parsedType,
				code: p.invalid_type
			}), v;
		}
		if (this._cache ||= new Set(this._def.values), !this._cache.has(e.data)) {
			let t = this._getOrReturnCtx(e), n = this._def.values;
			return g(t, {
				received: t.data,
				code: p.invalid_enum_value,
				options: n
			}), v;
		}
		return y(e.data);
	}
	get options() {
		return this._def.values;
	}
	get enum() {
		let e = {};
		for (let t of this._def.values) e[t] = t;
		return e;
	}
	get Values() {
		let e = {};
		for (let t of this._def.values) e[t] = t;
		return e;
	}
	get Enum() {
		let e = {};
		for (let t of this._def.values) e[t] = t;
		return e;
	}
	extract(t, n = this._def) {
		return e.create(t, {
			...this._def,
			...n
		});
	}
	exclude(t, n = this._def) {
		return e.create(this.options.filter((e) => !t.includes(e)), {
			...this._def,
			...n
		});
	}
};
st.create = ot;
var ct = class extends C {
	_parse(e) {
		let t = l.getValidEnumValues(this._def.values), n = this._getOrReturnCtx(e);
		if (n.parsedType !== d.string && n.parsedType !== d.number) {
			let e = l.objectValues(t);
			return g(n, {
				expected: l.joinValues(e),
				received: n.parsedType,
				code: p.invalid_type
			}), v;
		}
		if (this._cache ||= new Set(l.getValidEnumValues(this._def.values)), !this._cache.has(e.data)) {
			let e = l.objectValues(t);
			return g(n, {
				received: n.data,
				code: p.invalid_enum_value,
				options: e
			}), v;
		}
		return y(e.data);
	}
	get enum() {
		return this._def.values;
	}
};
ct.create = (e, t) => new ct({
	values: e,
	typeName: D.ZodNativeEnum,
	...S(t)
});
var lt = class extends C {
	unwrap() {
		return this._def.type;
	}
	_parse(e) {
		let { ctx: t } = this._processInputParams(e);
		return t.parsedType !== d.promise && t.common.async === !1 ? (g(t, {
			code: p.invalid_type,
			expected: d.promise,
			received: t.parsedType
		}), v) : y((t.parsedType === d.promise ? t.data : Promise.resolve(t.data)).then((e) => this._def.type.parseAsync(e, {
			path: t.path,
			errorMap: t.common.contextualErrorMap
		})));
	}
};
lt.create = (e, t) => new lt({
	type: e,
	typeName: D.ZodPromise,
	...S(t)
});
var T = class extends C {
	innerType() {
		return this._def.schema;
	}
	sourceType() {
		return this._def.schema._def.typeName === D.ZodEffects ? this._def.schema.sourceType() : this._def.schema;
	}
	_parse(e) {
		let { status: t, ctx: n } = this._processInputParams(e), r = this._def.effect || null, i = {
			addIssue: (e) => {
				g(n, e), e.fatal ? t.abort() : t.dirty();
			},
			get path() {
				return n.path;
			}
		};
		if (i.addIssue = i.addIssue.bind(i), r.type === "preprocess") {
			let e = r.transform(n.data, i);
			if (n.common.async) return Promise.resolve(e).then(async (e) => {
				if (t.value === "aborted") return v;
				let r = await this._def.schema._parseAsync({
					data: e,
					path: n.path,
					parent: n
				});
				return r.status === "aborted" ? v : r.status === "dirty" || t.value === "dirty" ? re(r.value) : r;
			});
			{
				if (t.value === "aborted") return v;
				let r = this._def.schema._parseSync({
					data: e,
					path: n.path,
					parent: n
				});
				return r.status === "aborted" ? v : r.status === "dirty" || t.value === "dirty" ? re(r.value) : r;
			}
		}
		if (r.type === "refinement") {
			let e = (e) => {
				let t = r.refinement(e, i);
				if (n.common.async) return Promise.resolve(t);
				if (t instanceof Promise) throw Error("Async refinement encountered during synchronous parse operation. Use .parseAsync instead.");
				return e;
			};
			if (n.common.async === !1) {
				let r = this._def.schema._parseSync({
					data: n.data,
					path: n.path,
					parent: n
				});
				return r.status === "aborted" ? v : (r.status === "dirty" && t.dirty(), e(r.value), {
					status: t.value,
					value: r.value
				});
			} else return this._def.schema._parseAsync({
				data: n.data,
				path: n.path,
				parent: n
			}).then((n) => n.status === "aborted" ? v : (n.status === "dirty" && t.dirty(), e(n.value).then(() => ({
				status: t.value,
				value: n.value
			}))));
		}
		if (r.type === "transform") if (n.common.async === !1) {
			let e = this._def.schema._parseSync({
				data: n.data,
				path: n.path,
				parent: n
			});
			if (!oe(e)) return v;
			let a = r.transform(e.value, i);
			if (a instanceof Promise) throw Error("Asynchronous transform encountered during synchronous parse operation. Use .parseAsync instead.");
			return {
				status: t.value,
				value: a
			};
		} else return this._def.schema._parseAsync({
			data: n.data,
			path: n.path,
			parent: n
		}).then((e) => oe(e) ? Promise.resolve(r.transform(e.value, i)).then((e) => ({
			status: t.value,
			value: e
		})) : v);
		l.assertNever(r);
	}
};
T.create = (e, t, n) => new T({
	schema: e,
	typeName: D.ZodEffects,
	effect: t,
	...S(n)
}), T.createWithPreprocess = (e, t, n) => new T({
	schema: t,
	effect: {
		type: "preprocess",
		transform: e
	},
	typeName: D.ZodEffects,
	...S(n)
});
var E = class extends C {
	_parse(e) {
		return this._getType(e) === d.undefined ? y(void 0) : this._def.innerType._parse(e);
	}
	unwrap() {
		return this._def.innerType;
	}
};
E.create = (e, t) => new E({
	innerType: e,
	typeName: D.ZodOptional,
	...S(t)
});
var ut = class extends C {
	_parse(e) {
		return this._getType(e) === d.null ? y(null) : this._def.innerType._parse(e);
	}
	unwrap() {
		return this._def.innerType;
	}
};
ut.create = (e, t) => new ut({
	innerType: e,
	typeName: D.ZodNullable,
	...S(t)
});
var dt = class extends C {
	_parse(e) {
		let { ctx: t } = this._processInputParams(e), n = t.data;
		return t.parsedType === d.undefined && (n = this._def.defaultValue()), this._def.innerType._parse({
			data: n,
			path: t.path,
			parent: t
		});
	}
	removeDefault() {
		return this._def.innerType;
	}
};
dt.create = (e, t) => new dt({
	innerType: e,
	typeName: D.ZodDefault,
	defaultValue: typeof t.default == "function" ? t.default : () => t.default,
	...S(t)
});
var ft = class extends C {
	_parse(e) {
		let { ctx: t } = this._processInputParams(e), n = {
			...t,
			common: {
				...t.common,
				issues: []
			}
		}, r = this._def.innerType._parse({
			data: n.data,
			path: n.path,
			parent: { ...n }
		});
		return se(r) ? r.then((e) => ({
			status: "valid",
			value: e.status === "valid" ? e.value : this._def.catchValue({
				get error() {
					return new m(n.common.issues);
				},
				input: n.data
			})
		})) : {
			status: "valid",
			value: r.status === "valid" ? r.value : this._def.catchValue({
				get error() {
					return new m(n.common.issues);
				},
				input: n.data
			})
		};
	}
	removeCatch() {
		return this._def.innerType;
	}
};
ft.create = (e, t) => new ft({
	innerType: e,
	typeName: D.ZodCatch,
	catchValue: typeof t.catch == "function" ? t.catch : () => t.catch,
	...S(t)
});
var pt = class extends C {
	_parse(e) {
		if (this._getType(e) !== d.nan) {
			let t = this._getOrReturnCtx(e);
			return g(t, {
				code: p.invalid_type,
				expected: d.nan,
				received: t.parsedType
			}), v;
		}
		return {
			status: "valid",
			value: e.data
		};
	}
};
pt.create = (e) => new pt({
	typeName: D.ZodNaN,
	...S(e)
});
var mt = class extends C {
	_parse(e) {
		let { ctx: t } = this._processInputParams(e), n = t.data;
		return this._def.type._parse({
			data: n,
			path: t.path,
			parent: t
		});
	}
	unwrap() {
		return this._def.type;
	}
}, ht = class e extends C {
	_parse(e) {
		let { status: t, ctx: n } = this._processInputParams(e);
		if (n.common.async) return (async () => {
			let e = await this._def.in._parseAsync({
				data: n.data,
				path: n.path,
				parent: n
			});
			return e.status === "aborted" ? v : e.status === "dirty" ? (t.dirty(), re(e.value)) : this._def.out._parseAsync({
				data: e.value,
				path: n.path,
				parent: n
			});
		})();
		{
			let e = this._def.in._parseSync({
				data: n.data,
				path: n.path,
				parent: n
			});
			return e.status === "aborted" ? v : e.status === "dirty" ? (t.dirty(), {
				status: "dirty",
				value: e.value
			}) : this._def.out._parseSync({
				data: e.value,
				path: n.path,
				parent: n
			});
		}
	}
	static create(t, n) {
		return new e({
			in: t,
			out: n,
			typeName: D.ZodPipeline
		});
	}
}, gt = class extends C {
	_parse(e) {
		let t = this._def.innerType._parse(e), n = (e) => (oe(e) && (e.value = Object.freeze(e.value)), e);
		return se(t) ? t.then((e) => n(e)) : n(t);
	}
	unwrap() {
		return this._def.innerType;
	}
};
gt.create = (e, t) => new gt({
	innerType: e,
	typeName: D.ZodReadonly,
	...S(t)
}), w.lazycreate;
var D;
(function(e) {
	e.ZodString = "ZodString", e.ZodNumber = "ZodNumber", e.ZodNaN = "ZodNaN", e.ZodBigInt = "ZodBigInt", e.ZodBoolean = "ZodBoolean", e.ZodDate = "ZodDate", e.ZodSymbol = "ZodSymbol", e.ZodUndefined = "ZodUndefined", e.ZodNull = "ZodNull", e.ZodAny = "ZodAny", e.ZodUnknown = "ZodUnknown", e.ZodNever = "ZodNever", e.ZodVoid = "ZodVoid", e.ZodArray = "ZodArray", e.ZodObject = "ZodObject", e.ZodUnion = "ZodUnion", e.ZodDiscriminatedUnion = "ZodDiscriminatedUnion", e.ZodIntersection = "ZodIntersection", e.ZodTuple = "ZodTuple", e.ZodRecord = "ZodRecord", e.ZodMap = "ZodMap", e.ZodSet = "ZodSet", e.ZodFunction = "ZodFunction", e.ZodLazy = "ZodLazy", e.ZodLiteral = "ZodLiteral", e.ZodEnum = "ZodEnum", e.ZodEffects = "ZodEffects", e.ZodNativeEnum = "ZodNativeEnum", e.ZodOptional = "ZodOptional", e.ZodNullable = "ZodNullable", e.ZodDefault = "ZodDefault", e.ZodCatch = "ZodCatch", e.ZodPromise = "ZodPromise", e.ZodBranded = "ZodBranded", e.ZodPipeline = "ZodPipeline", e.ZodReadonly = "ZodReadonly";
})(D ||= {});
var O = Ne.create, _t = Fe.create;
pt.create, Ie.create;
var k = Le.create;
Re.create, ze.create, Be.create, Ve.create;
var A = He.create, vt = Ue.create;
We.create, Ge.create;
var j = Ke.create, M = w.create;
w.strictCreate;
var yt = Je.create, bt = Xe.create;
Qe.create, $e.create;
var xt = et.create;
tt.create, nt.create, rt.create, it.create;
var N = at.create, St = st.create, Ct = ct.create;
lt.create, T.create, E.create, ut.create, T.createWithPreprocess, ht.create;
//#endregion
//#region node_modules/@ag-ui/core/dist/index.mjs
var wt = M({
	name: O(),
	arguments: O()
}), Tt = M({
	id: O(),
	type: N("function"),
	function: wt,
	encryptedValue: O().optional()
}), Et = M({
	id: O(),
	role: O(),
	content: O().optional(),
	name: O().optional(),
	encryptedValue: O().optional()
}), Dt = M({
	type: N("text"),
	text: O()
}), Ot = bt("type", [M({
	type: N("data"),
	value: O(),
	mimeType: O()
}), M({
	type: N("url"),
	value: O(),
	mimeType: O().optional()
})]), kt = M({
	type: N("image"),
	source: Ot,
	metadata: vt().optional()
}), At = M({
	type: N("audio"),
	source: Ot,
	metadata: vt().optional()
}), jt = M({
	type: N("video"),
	source: Ot,
	metadata: vt().optional()
}), Mt = M({
	type: N("document"),
	source: Ot,
	metadata: vt().optional()
}), Nt = M({
	type: N("binary"),
	mimeType: O(),
	id: O().optional(),
	url: O().optional(),
	data: O().optional(),
	filename: O().optional()
}), Pt = (e, t) => {
	!e.id && !e.url && !e.data && t.addIssue({
		code: p.custom,
		message: "BinaryInputContent requires at least one of id, url, or data.",
		path: ["id"]
	});
};
Nt.superRefine((e, t) => {
	Pt(e, t);
});
var Ft = bt("type", [
	Dt,
	kt,
	At,
	jt,
	Mt,
	Nt
]).superRefine((e, t) => {
	e.type === "binary" && Pt(e, t);
}), It = bt("role", [
	Et.extend({
		role: N("developer"),
		content: O()
	}),
	Et.extend({
		role: N("system"),
		content: O()
	}),
	Et.extend({
		role: N("assistant"),
		content: O().optional(),
		toolCalls: j(Tt).optional()
	}),
	Et.extend({
		role: N("user"),
		content: yt([O(), j(Ft)])
	}),
	M({
		id: O(),
		content: O(),
		role: N("tool"),
		toolCallId: O(),
		error: O().optional(),
		encryptedValue: O().optional()
	}),
	M({
		id: O(),
		role: N("activity"),
		activityType: O(),
		content: xt(A())
	}),
	M({
		id: O(),
		role: N("reasoning"),
		content: O(),
		encryptedValue: O().optional()
	})
]);
yt([
	N("developer"),
	N("system"),
	N("assistant"),
	N("user"),
	N("tool"),
	N("activity"),
	N("reasoning")
]);
var Lt = M({
	description: O(),
	value: O()
}), Rt = M({
	name: O(),
	description: O(),
	parameters: A(),
	metadata: xt(A()).optional()
}), zt = M({
	threadId: O(),
	runId: O(),
	parentRunId: O().optional(),
	state: A(),
	messages: j(It),
	tools: j(Rt),
	context: j(Lt),
	forwardedProps: A()
}), Bt = A(), P = class extends Error {
	constructor(e) {
		super(e);
	}
}, Vt = class extends P {
	constructor() {
		super("Connect not implemented. This method is not supported by the current agent.");
	}
}, Ht = M({
	name: O(),
	description: O().optional()
}), Ut = M({
	name: O().optional(),
	type: O().optional(),
	description: O().optional(),
	version: O().optional(),
	provider: O().optional(),
	documentationUrl: O().optional(),
	metadata: xt(vt()).optional()
}), Wt = M({
	streaming: k().optional(),
	websocket: k().optional(),
	httpBinary: k().optional(),
	pushNotifications: k().optional(),
	resumable: k().optional()
}), Gt = M({
	supported: k().optional(),
	items: j(Rt).optional(),
	parallelCalls: k().optional(),
	clientProvided: k().optional()
}), Kt = M({
	structuredOutput: k().optional(),
	supportedMimeTypes: j(O()).optional()
}), qt = M({
	snapshots: k().optional(),
	deltas: k().optional(),
	memory: k().optional(),
	persistentState: k().optional()
}), Jt = M({
	supported: k().optional(),
	delegation: k().optional(),
	handoffs: k().optional(),
	subAgents: j(Ht).optional()
}), Yt = M({
	supported: k().optional(),
	streaming: k().optional(),
	encrypted: k().optional()
}), Xt = M({
	image: k().optional(),
	audio: k().optional(),
	video: k().optional(),
	pdf: k().optional(),
	file: k().optional()
}), Zt = M({
	image: k().optional(),
	audio: k().optional()
}), Qt = M({
	input: Xt.optional(),
	output: Zt.optional()
}), $t = M({
	codeExecution: k().optional(),
	sandboxed: k().optional(),
	maxIterations: _t().optional(),
	maxExecutionTime: _t().optional()
}), en = M({
	supported: k().optional(),
	approvals: k().optional(),
	interventions: k().optional(),
	feedback: k().optional()
});
M({
	identity: Ut.optional(),
	transport: Wt.optional(),
	tools: Gt.optional(),
	output: Kt.optional(),
	state: qt.optional(),
	multiAgent: Jt.optional(),
	reasoning: Yt.optional(),
	multimodal: Qt.optional(),
	execution: $t.optional(),
	humanInTheLoop: en.optional(),
	custom: xt(vt()).optional()
});
var tn = yt([
	N("developer"),
	N("system"),
	N("assistant"),
	N("user")
]), F = /* @__PURE__ */ function(e) {
	return e.TEXT_MESSAGE_START = "TEXT_MESSAGE_START", e.TEXT_MESSAGE_CONTENT = "TEXT_MESSAGE_CONTENT", e.TEXT_MESSAGE_END = "TEXT_MESSAGE_END", e.TEXT_MESSAGE_CHUNK = "TEXT_MESSAGE_CHUNK", e.TOOL_CALL_START = "TOOL_CALL_START", e.TOOL_CALL_ARGS = "TOOL_CALL_ARGS", e.TOOL_CALL_END = "TOOL_CALL_END", e.TOOL_CALL_CHUNK = "TOOL_CALL_CHUNK", e.TOOL_CALL_RESULT = "TOOL_CALL_RESULT", e.THINKING_START = "THINKING_START", e.THINKING_END = "THINKING_END", e.THINKING_TEXT_MESSAGE_START = "THINKING_TEXT_MESSAGE_START", e.THINKING_TEXT_MESSAGE_CONTENT = "THINKING_TEXT_MESSAGE_CONTENT", e.THINKING_TEXT_MESSAGE_END = "THINKING_TEXT_MESSAGE_END", e.STATE_SNAPSHOT = "STATE_SNAPSHOT", e.STATE_DELTA = "STATE_DELTA", e.MESSAGES_SNAPSHOT = "MESSAGES_SNAPSHOT", e.ACTIVITY_SNAPSHOT = "ACTIVITY_SNAPSHOT", e.ACTIVITY_DELTA = "ACTIVITY_DELTA", e.RAW = "RAW", e.CUSTOM = "CUSTOM", e.RUN_STARTED = "RUN_STARTED", e.RUN_FINISHED = "RUN_FINISHED", e.RUN_ERROR = "RUN_ERROR", e.STEP_STARTED = "STEP_STARTED", e.STEP_FINISHED = "STEP_FINISHED", e.REASONING_START = "REASONING_START", e.REASONING_MESSAGE_START = "REASONING_MESSAGE_START", e.REASONING_MESSAGE_CONTENT = "REASONING_MESSAGE_CONTENT", e.REASONING_MESSAGE_END = "REASONING_MESSAGE_END", e.REASONING_MESSAGE_CHUNK = "REASONING_MESSAGE_CHUNK", e.REASONING_END = "REASONING_END", e.REASONING_ENCRYPTED_VALUE = "REASONING_ENCRYPTED_VALUE", e;
}({}), I = M({
	type: Ct(F),
	timestamp: _t().optional(),
	rawEvent: A().optional()
}).passthrough(), nn = I.extend({
	type: N(F.TEXT_MESSAGE_START),
	messageId: O(),
	role: tn.default("assistant"),
	name: O().optional()
}), rn = I.extend({
	type: N(F.TEXT_MESSAGE_CONTENT),
	messageId: O(),
	delta: O()
}), an = I.extend({
	type: N(F.TEXT_MESSAGE_END),
	messageId: O()
}), on = I.extend({
	type: N(F.TEXT_MESSAGE_CHUNK),
	messageId: O().optional(),
	role: tn.optional(),
	delta: O().optional(),
	name: O().optional()
}), sn = I.extend({ type: N(F.THINKING_TEXT_MESSAGE_START) }), cn = rn.omit({
	messageId: !0,
	type: !0
}).extend({ type: N(F.THINKING_TEXT_MESSAGE_CONTENT) }), ln = I.extend({ type: N(F.THINKING_TEXT_MESSAGE_END) }), un = I.extend({
	type: N(F.TOOL_CALL_START),
	toolCallId: O(),
	toolCallName: O(),
	parentMessageId: O().optional()
}), dn = I.extend({
	type: N(F.TOOL_CALL_ARGS),
	toolCallId: O(),
	delta: O()
}), fn = I.extend({
	type: N(F.TOOL_CALL_END),
	toolCallId: O()
}), pn = I.extend({
	messageId: O(),
	type: N(F.TOOL_CALL_RESULT),
	toolCallId: O(),
	content: O(),
	role: N("tool").optional()
}), mn = I.extend({
	type: N(F.TOOL_CALL_CHUNK),
	toolCallId: O().optional(),
	toolCallName: O().optional(),
	parentMessageId: O().optional(),
	delta: O().optional()
}), hn = I.extend({
	type: N(F.THINKING_START),
	title: O().optional()
}), gn = I.extend({ type: N(F.THINKING_END) }), _n = I.extend({
	type: N(F.STATE_SNAPSHOT),
	snapshot: Bt
}), vn = I.extend({
	type: N(F.STATE_DELTA),
	delta: j(A())
}), yn = I.extend({
	type: N(F.MESSAGES_SNAPSHOT),
	messages: j(It)
}), bn = I.extend({
	type: N(F.ACTIVITY_SNAPSHOT),
	messageId: O(),
	activityType: O(),
	content: xt(A()),
	replace: k().optional().default(!0)
}), xn = I.extend({
	type: N(F.ACTIVITY_DELTA),
	messageId: O(),
	activityType: O(),
	patch: j(A())
}), Sn = I.extend({
	type: N(F.RAW),
	event: A(),
	source: O().optional()
}), Cn = I.extend({
	type: N(F.CUSTOM),
	name: O(),
	value: A()
}), wn = I.extend({
	type: N(F.RUN_STARTED),
	threadId: O(),
	runId: O(),
	parentRunId: O().optional(),
	input: zt.optional()
}), Tn = I.extend({
	type: N(F.RUN_FINISHED),
	threadId: O(),
	runId: O(),
	result: A().optional()
}), En = I.extend({
	type: N(F.RUN_ERROR),
	message: O(),
	code: O().optional()
}), Dn = I.extend({
	type: N(F.STEP_STARTED),
	stepName: O()
}), On = I.extend({
	type: N(F.STEP_FINISHED),
	stepName: O()
}), kn = yt([N("tool-call"), N("message")]), An = bt("type", [
	nn,
	rn,
	an,
	on,
	hn,
	gn,
	sn,
	cn,
	ln,
	un,
	dn,
	fn,
	mn,
	pn,
	_n,
	vn,
	yn,
	bn,
	xn,
	Sn,
	Cn,
	wn,
	Tn,
	En,
	Dn,
	On,
	I.extend({
		type: N(F.REASONING_START),
		messageId: O()
	}),
	I.extend({
		type: N(F.REASONING_MESSAGE_START),
		messageId: O(),
		role: N("reasoning")
	}),
	I.extend({
		type: N(F.REASONING_MESSAGE_CONTENT),
		messageId: O(),
		delta: O()
	}),
	I.extend({
		type: N(F.REASONING_MESSAGE_END),
		messageId: O()
	}),
	I.extend({
		type: N(F.REASONING_MESSAGE_CHUNK),
		messageId: O().optional(),
		delta: O().optional()
	}),
	I.extend({
		type: N(F.REASONING_END),
		messageId: O()
	}),
	I.extend({
		type: N(F.REASONING_ENCRYPTED_VALUE),
		subtype: kn,
		entityId: O(),
		encryptedValue: O()
	})
]), jn = (function() {
	var e = function(t, n) {
		return e = Object.setPrototypeOf || { __proto__: [] } instanceof Array && function(e, t) {
			e.__proto__ = t;
		} || function(e, t) {
			for (var n in t) t.hasOwnProperty(n) && (e[n] = t[n]);
		}, e(t, n);
	};
	return function(t, n) {
		e(t, n);
		function r() {
			this.constructor = t;
		}
		t.prototype = n === null ? Object.create(n) : (r.prototype = n.prototype, new r());
	};
})(), Mn = Object.prototype.hasOwnProperty;
function Nn(e, t) {
	return Mn.call(e, t);
}
function Pn(e) {
	if (Array.isArray(e)) {
		for (var t = Array(e.length), n = 0; n < t.length; n++) t[n] = "" + n;
		return t;
	}
	if (Object.keys) return Object.keys(e);
	var r = [];
	for (var i in e) Nn(e, i) && r.push(i);
	return r;
}
function L(e) {
	switch (typeof e) {
		case "object": return JSON.parse(JSON.stringify(e));
		case "undefined": return null;
		default: return e;
	}
}
function Fn(e) {
	for (var t = 0, n = e.length, r; t < n;) {
		if (r = e.charCodeAt(t), r >= 48 && r <= 57) {
			t++;
			continue;
		}
		return !1;
	}
	return !0;
}
function In(e) {
	return e.indexOf("/") === -1 && e.indexOf("~") === -1 ? e : e.replace(/~/g, "~0").replace(/\//g, "~1");
}
function Ln(e) {
	return e.replace(/~1/g, "/").replace(/~0/g, "~");
}
function Rn(e) {
	if (e === void 0) return !0;
	if (e) {
		if (Array.isArray(e)) {
			for (var t = 0, n = e.length; t < n; t++) if (Rn(e[t])) return !0;
		} else if (typeof e == "object") {
			for (var r = Pn(e), i = r.length, a = 0; a < i; a++) if (Rn(e[r[a]])) return !0;
		}
	}
	return !1;
}
function zn(e, t) {
	var n = [e];
	for (var r in t) {
		var i = typeof t[r] == "object" ? JSON.stringify(t[r], null, 2) : t[r];
		i !== void 0 && n.push(r + ": " + i);
	}
	return n.join("\n");
}
var Bn = function(e) {
	jn(t, e);
	function t(t, n, r, i, a) {
		var o = this.constructor, s = e.call(this, zn(t, {
			name: n,
			index: r,
			operation: i,
			tree: a
		})) || this;
		return s.name = n, s.index = r, s.operation = i, s.tree = a, Object.setPrototypeOf(s, o.prototype), s.message = zn(t, {
			name: n,
			index: r,
			operation: i,
			tree: a
		}), s;
	}
	return t;
}(Error), Vn = /* @__PURE__ */ t({
	JsonPatchError: () => R,
	_areEquals: () => Zn,
	applyOperation: () => Kn,
	applyPatch: () => qn,
	applyReducer: () => Jn,
	deepClone: () => Hn,
	getValueByPointer: () => Gn,
	validate: () => Xn,
	validator: () => Yn
}), R = Bn, Hn = L, Un = {
	add: function(e, t, n) {
		return e[t] = this.value, { newDocument: n };
	},
	remove: function(e, t, n) {
		var r = e[t];
		return delete e[t], {
			newDocument: n,
			removed: r
		};
	},
	replace: function(e, t, n) {
		var r = e[t];
		return e[t] = this.value, {
			newDocument: n,
			removed: r
		};
	},
	move: function(e, t, n) {
		var r = Gn(n, this.path);
		r &&= L(r);
		var i = Kn(n, {
			op: "remove",
			path: this.from
		}).removed;
		return Kn(n, {
			op: "add",
			path: this.path,
			value: i
		}), {
			newDocument: n,
			removed: r
		};
	},
	copy: function(e, t, n) {
		var r = Gn(n, this.from);
		return Kn(n, {
			op: "add",
			path: this.path,
			value: L(r)
		}), { newDocument: n };
	},
	test: function(e, t, n) {
		return {
			newDocument: n,
			test: Zn(e[t], this.value)
		};
	},
	_get: function(e, t, n) {
		return this.value = e[t], { newDocument: n };
	}
}, Wn = {
	add: function(e, t, n) {
		return Fn(t) ? e.splice(t, 0, this.value) : e[t] = this.value, {
			newDocument: n,
			index: t
		};
	},
	remove: function(e, t, n) {
		return {
			newDocument: n,
			removed: e.splice(t, 1)[0]
		};
	},
	replace: function(e, t, n) {
		var r = e[t];
		return e[t] = this.value, {
			newDocument: n,
			removed: r
		};
	},
	move: Un.move,
	copy: Un.copy,
	test: Un.test,
	_get: Un._get
};
function Gn(e, t) {
	if (t == "") return e;
	var n = {
		op: "_get",
		path: t
	};
	return Kn(e, n), n.value;
}
function Kn(e, t, n, r, i, a) {
	if (n === void 0 && (n = !1), r === void 0 && (r = !0), i === void 0 && (i = !0), a === void 0 && (a = 0), n && (typeof n == "function" ? n(t, 0, e, t.path) : Yn(t, 0)), t.path === "") {
		var o = { newDocument: e };
		if (t.op === "add") return o.newDocument = t.value, o;
		if (t.op === "replace") return o.newDocument = t.value, o.removed = e, o;
		if (t.op === "move" || t.op === "copy") return o.newDocument = Gn(e, t.from), t.op === "move" && (o.removed = e), o;
		if (t.op === "test") {
			if (o.test = Zn(e, t.value), o.test === !1) throw new R("Test operation failed", "TEST_OPERATION_FAILED", a, t, e);
			return o.newDocument = e, o;
		} else if (t.op === "remove") return o.removed = e, o.newDocument = null, o;
		else if (t.op === "_get") return t.value = e, o;
		else if (n) throw new R("Operation `op` property is not one of operations defined in RFC-6902", "OPERATION_OP_INVALID", a, t, e);
		else return o;
	} else {
		r || (e = L(e));
		var s = (t.path || "").split("/"), c = e, l = 1, u = s.length, d = void 0, f = void 0, p = void 0;
		for (p = typeof n == "function" ? n : Yn;;) {
			if (f = s[l], f && f.indexOf("~") != -1 && (f = Ln(f)), i && (f == "__proto__" || f == "prototype" && l > 0 && s[l - 1] == "constructor")) throw TypeError("JSON-Patch: modifying `__proto__` or `constructor/prototype` prop is banned for security reasons, if this was on purpose, please set `banPrototypeModifications` flag false and pass it to this function. More info in fast-json-patch README");
			if (n && d === void 0 && (c[f] === void 0 ? d = s.slice(0, l).join("/") : l == u - 1 && (d = t.path), d !== void 0 && p(t, 0, e, d)), l++, Array.isArray(c)) {
				if (f === "-") f = c.length;
				else if (n && !Fn(f)) throw new R("Expected an unsigned base-10 integer value, making the new referenced value the array element with the zero-based index", "OPERATION_PATH_ILLEGAL_ARRAY_INDEX", a, t, e);
				else Fn(f) && (f = ~~f);
				if (l >= u) {
					if (n && t.op === "add" && f > c.length) throw new R("The specified index MUST NOT be greater than the number of elements in the array", "OPERATION_VALUE_OUT_OF_BOUNDS", a, t, e);
					var o = Wn[t.op].call(t, c, f, e);
					if (o.test === !1) throw new R("Test operation failed", "TEST_OPERATION_FAILED", a, t, e);
					return o;
				}
			} else if (l >= u) {
				var o = Un[t.op].call(t, c, f, e);
				if (o.test === !1) throw new R("Test operation failed", "TEST_OPERATION_FAILED", a, t, e);
				return o;
			}
			if (c = c[f], n && l < u && (!c || typeof c != "object")) throw new R("Cannot perform operation at the desired path", "OPERATION_PATH_UNRESOLVABLE", a, t, e);
		}
	}
}
function qn(e, t, n, r, i) {
	if (r === void 0 && (r = !0), i === void 0 && (i = !0), n && !Array.isArray(t)) throw new R("Patch sequence must be an array", "SEQUENCE_NOT_AN_ARRAY");
	r || (e = L(e));
	for (var a = Array(t.length), o = 0, s = t.length; o < s; o++) a[o] = Kn(e, t[o], n, !0, i, o), e = a[o].newDocument;
	return a.newDocument = e, a;
}
function Jn(e, t, n) {
	var r = Kn(e, t);
	if (r.test === !1) throw new R("Test operation failed", "TEST_OPERATION_FAILED", n, t, e);
	return r.newDocument;
}
function Yn(e, t, n, r) {
	if (typeof e != "object" || !e || Array.isArray(e)) throw new R("Operation is not an object", "OPERATION_NOT_AN_OBJECT", t, e, n);
	if (!Un[e.op]) throw new R("Operation `op` property is not one of operations defined in RFC-6902", "OPERATION_OP_INVALID", t, e, n);
	if (typeof e.path != "string") throw new R("Operation `path` property is not a string", "OPERATION_PATH_INVALID", t, e, n);
	if (e.path.indexOf("/") !== 0 && e.path.length > 0) throw new R("Operation `path` property must start with \"/\"", "OPERATION_PATH_INVALID", t, e, n);
	if ((e.op === "move" || e.op === "copy") && typeof e.from != "string") throw new R("Operation `from` property is not present (applicable in `move` and `copy` operations)", "OPERATION_FROM_REQUIRED", t, e, n);
	if ((e.op === "add" || e.op === "replace" || e.op === "test") && e.value === void 0) throw new R("Operation `value` property is not present (applicable in `add`, `replace` and `test` operations)", "OPERATION_VALUE_REQUIRED", t, e, n);
	if ((e.op === "add" || e.op === "replace" || e.op === "test") && Rn(e.value)) throw new R("Operation `value` property is not present (applicable in `add`, `replace` and `test` operations)", "OPERATION_VALUE_CANNOT_CONTAIN_UNDEFINED", t, e, n);
	if (n) {
		if (e.op == "add") {
			var i = e.path.split("/").length, a = r.split("/").length;
			if (i !== a + 1 && i !== a) throw new R("Cannot perform an `add` operation at the desired path", "OPERATION_PATH_CANNOT_ADD", t, e, n);
		} else if (e.op === "replace" || e.op === "remove" || e.op === "_get") {
			if (e.path !== r) throw new R("Cannot perform the operation at a path that does not exist", "OPERATION_PATH_UNRESOLVABLE", t, e, n);
		} else if (e.op === "move" || e.op === "copy") {
			var o = Xn([{
				op: "_get",
				path: e.from,
				value: void 0
			}], n);
			if (o && o.name === "OPERATION_PATH_UNRESOLVABLE") throw new R("Cannot perform the operation from a path that does not exist", "OPERATION_FROM_UNRESOLVABLE", t, e, n);
		}
	}
}
function Xn(e, t, n) {
	try {
		if (!Array.isArray(e)) throw new R("Patch sequence must be an array", "SEQUENCE_NOT_AN_ARRAY");
		if (t) qn(L(t), L(e), n || !0);
		else {
			n ||= Yn;
			for (var r = 0; r < e.length; r++) n(e[r], r, t, void 0);
		}
	} catch (e) {
		if (e instanceof R) return e;
		throw e;
	}
}
function Zn(e, t) {
	if (e === t) return !0;
	if (e && t && typeof e == "object" && typeof t == "object") {
		var n = Array.isArray(e), r = Array.isArray(t), i, a, o;
		if (n && r) {
			if (a = e.length, a != t.length) return !1;
			for (i = a; i-- !== 0;) if (!Zn(e[i], t[i])) return !1;
			return !0;
		}
		if (n != r) return !1;
		var s = Object.keys(e);
		if (a = s.length, a !== Object.keys(t).length) return !1;
		for (i = a; i-- !== 0;) if (!t.hasOwnProperty(s[i])) return !1;
		for (i = a; i-- !== 0;) if (o = s[i], !Zn(e[o], t[o])) return !1;
		return !0;
	}
	return e !== e && t !== t;
}
//#endregion
//#region node_modules/fast-json-patch/module/duplex.mjs
var Qn = /* @__PURE__ */ t({
	compare: () => lr,
	generate: () => sr,
	observe: () => or,
	unobserve: () => ar
}), $n = /* @__PURE__ */ new WeakMap(), er = function() {
	function e(e) {
		this.observers = /* @__PURE__ */ new Map(), this.obj = e;
	}
	return e;
}(), tr = function() {
	function e(e, t) {
		this.callback = e, this.observer = t;
	}
	return e;
}();
function nr(e) {
	return $n.get(e);
}
function rr(e, t) {
	return e.observers.get(t);
}
function ir(e, t) {
	e.observers.delete(t.callback);
}
function ar(e, t) {
	t.unobserve();
}
function or(e, t) {
	var n = [], r, i = nr(e);
	if (!i) i = new er(e), $n.set(e, i);
	else {
		var a = rr(i, t);
		r = a && a.observer;
	}
	if (r) return r;
	if (r = {}, i.value = L(e), t) {
		r.callback = t, r.next = null;
		var o = function() {
			sr(r);
		}, s = function() {
			clearTimeout(r.next), r.next = setTimeout(o);
		};
		typeof window < "u" && (window.addEventListener("mouseup", s), window.addEventListener("keyup", s), window.addEventListener("mousedown", s), window.addEventListener("keydown", s), window.addEventListener("change", s));
	}
	return r.patches = n, r.object = e, r.unobserve = function() {
		sr(r), clearTimeout(r.next), ir(i, r), typeof window < "u" && (window.removeEventListener("mouseup", s), window.removeEventListener("keyup", s), window.removeEventListener("mousedown", s), window.removeEventListener("keydown", s), window.removeEventListener("change", s));
	}, i.observers.set(t, new tr(t, r)), r;
}
function sr(e, t) {
	t === void 0 && (t = !1);
	var n = $n.get(e.object);
	cr(n.value, e.object, e.patches, "", t), e.patches.length && qn(n.value, e.patches);
	var r = e.patches;
	return r.length > 0 && (e.patches = [], e.callback && e.callback(r)), r;
}
function cr(e, t, n, r, i) {
	if (t !== e) {
		typeof t.toJSON == "function" && (t = t.toJSON());
		for (var a = Pn(t), o = Pn(e), s = !1, c = o.length - 1; c >= 0; c--) {
			var l = o[c], u = e[l];
			if (Nn(t, l) && !(t[l] === void 0 && u !== void 0 && Array.isArray(t) === !1)) {
				var d = t[l];
				typeof u == "object" && u && typeof d == "object" && d && Array.isArray(u) === Array.isArray(d) ? cr(u, d, n, r + "/" + In(l), i) : u !== d && (i && n.push({
					op: "test",
					path: r + "/" + In(l),
					value: L(u)
				}), n.push({
					op: "replace",
					path: r + "/" + In(l),
					value: L(d)
				}));
			} else Array.isArray(e) === Array.isArray(t) ? (i && n.push({
				op: "test",
				path: r + "/" + In(l),
				value: L(u)
			}), n.push({
				op: "remove",
				path: r + "/" + In(l)
			}), s = !0) : (i && n.push({
				op: "test",
				path: r,
				value: e
			}), n.push({
				op: "replace",
				path: r,
				value: t
			}));
		}
		if (!(!s && a.length == o.length)) for (var c = 0; c < a.length; c++) {
			var l = a[c];
			!Nn(e, l) && t[l] !== void 0 && n.push({
				op: "add",
				path: r + "/" + In(l),
				value: L(t[l])
			});
		}
	}
}
function lr(e, t, n) {
	n === void 0 && (n = !1);
	var r = [];
	return cr(e, t, r, "", n), r;
}
//#endregion
//#region node_modules/fast-json-patch/index.mjs
var ur = Object.assign({}, Vn, Qn, {
	JsonPatchError: Bn,
	deepClone: L,
	escapePathComponent: In,
	unescapePathComponent: Ln
}), dr = function(e, t) {
	return dr = Object.setPrototypeOf || { __proto__: [] } instanceof Array && function(e, t) {
		e.__proto__ = t;
	} || function(e, t) {
		for (var n in t) Object.prototype.hasOwnProperty.call(t, n) && (e[n] = t[n]);
	}, dr(e, t);
};
function fr(e, t) {
	if (typeof t != "function" && t !== null) throw TypeError("Class extends value " + String(t) + " is not a constructor or null");
	dr(e, t);
	function n() {
		this.constructor = e;
	}
	e.prototype = t === null ? Object.create(t) : (n.prototype = t.prototype, new n());
}
function pr(e, t, n, r) {
	function i(e) {
		return e instanceof n ? e : new n(function(t) {
			t(e);
		});
	}
	return new (n ||= Promise)(function(n, a) {
		function o(e) {
			try {
				c(r.next(e));
			} catch (e) {
				a(e);
			}
		}
		function s(e) {
			try {
				c(r.throw(e));
			} catch (e) {
				a(e);
			}
		}
		function c(e) {
			e.done ? n(e.value) : i(e.value).then(o, s);
		}
		c((r = r.apply(e, t || [])).next());
	});
}
function mr(e, t) {
	var n = {
		label: 0,
		sent: function() {
			if (a[0] & 1) throw a[1];
			return a[1];
		},
		trys: [],
		ops: []
	}, r, i, a, o = Object.create((typeof Iterator == "function" ? Iterator : Object).prototype);
	return o.next = s(0), o.throw = s(1), o.return = s(2), typeof Symbol == "function" && (o[Symbol.iterator] = function() {
		return this;
	}), o;
	function s(e) {
		return function(t) {
			return c([e, t]);
		};
	}
	function c(s) {
		if (r) throw TypeError("Generator is already executing.");
		for (; o && (o = 0, s[0] && (n = 0)), n;) try {
			if (r = 1, i && (a = s[0] & 2 ? i.return : s[0] ? i.throw || ((a = i.return) && a.call(i), 0) : i.next) && !(a = a.call(i, s[1])).done) return a;
			switch (i = 0, a && (s = [s[0] & 2, a.value]), s[0]) {
				case 0:
				case 1:
					a = s;
					break;
				case 4: return n.label++, {
					value: s[1],
					done: !1
				};
				case 5:
					n.label++, i = s[1], s = [0];
					continue;
				case 7:
					s = n.ops.pop(), n.trys.pop();
					continue;
				default:
					if ((a = n.trys, !(a = a.length > 0 && a[a.length - 1])) && (s[0] === 6 || s[0] === 2)) {
						n = 0;
						continue;
					}
					if (s[0] === 3 && (!a || s[1] > a[0] && s[1] < a[3])) {
						n.label = s[1];
						break;
					}
					if (s[0] === 6 && n.label < a[1]) {
						n.label = a[1], a = s;
						break;
					}
					if (a && n.label < a[2]) {
						n.label = a[2], n.ops.push(s);
						break;
					}
					a[2] && n.ops.pop(), n.trys.pop();
					continue;
			}
			s = t.call(e, n);
		} catch (e) {
			s = [6, e], i = 0;
		} finally {
			r = a = 0;
		}
		if (s[0] & 5) throw s[1];
		return {
			value: s[0] ? s[1] : void 0,
			done: !0
		};
	}
}
function hr(e) {
	var t = typeof Symbol == "function" && Symbol.iterator, n = t && e[t], r = 0;
	if (n) return n.call(e);
	if (e && typeof e.length == "number") return { next: function() {
		return e && r >= e.length && (e = void 0), {
			value: e && e[r++],
			done: !e
		};
	} };
	throw TypeError(t ? "Object is not iterable." : "Symbol.iterator is not defined.");
}
function gr(e, t) {
	var n = typeof Symbol == "function" && e[Symbol.iterator];
	if (!n) return e;
	var r = n.call(e), i, a = [], o;
	try {
		for (; (t === void 0 || t-- > 0) && !(i = r.next()).done;) a.push(i.value);
	} catch (e) {
		o = { error: e };
	} finally {
		try {
			i && !i.done && (n = r.return) && n.call(r);
		} finally {
			if (o) throw o.error;
		}
	}
	return a;
}
function _r(e, t, n) {
	if (n || arguments.length === 2) for (var r = 0, i = t.length, a; r < i; r++) (a || !(r in t)) && (a ||= Array.prototype.slice.call(t, 0, r), a[r] = t[r]);
	return e.concat(a || Array.prototype.slice.call(t));
}
function vr(e) {
	return this instanceof vr ? (this.v = e, this) : new vr(e);
}
function yr(e, t, n) {
	if (!Symbol.asyncIterator) throw TypeError("Symbol.asyncIterator is not defined.");
	var r = n.apply(e, t || []), i, a = [];
	return i = Object.create((typeof AsyncIterator == "function" ? AsyncIterator : Object).prototype), s("next"), s("throw"), s("return", o), i[Symbol.asyncIterator] = function() {
		return this;
	}, i;
	function o(e) {
		return function(t) {
			return Promise.resolve(t).then(e, d);
		};
	}
	function s(e, t) {
		r[e] && (i[e] = function(t) {
			return new Promise(function(n, r) {
				a.push([
					e,
					t,
					n,
					r
				]) > 1 || c(e, t);
			});
		}, t && (i[e] = t(i[e])));
	}
	function c(e, t) {
		try {
			l(r[e](t));
		} catch (e) {
			f(a[0][3], e);
		}
	}
	function l(e) {
		e.value instanceof vr ? Promise.resolve(e.value.v).then(u, d) : f(a[0][2], e);
	}
	function u(e) {
		c("next", e);
	}
	function d(e) {
		c("throw", e);
	}
	function f(e, t) {
		e(t), a.shift(), a.length && c(a[0][0], a[0][1]);
	}
}
function br(e) {
	if (!Symbol.asyncIterator) throw TypeError("Symbol.asyncIterator is not defined.");
	var t = e[Symbol.asyncIterator], n;
	return t ? t.call(e) : (e = typeof hr == "function" ? hr(e) : e[Symbol.iterator](), n = {}, r("next"), r("throw"), r("return"), n[Symbol.asyncIterator] = function() {
		return this;
	}, n);
	function r(t) {
		n[t] = e[t] && function(n) {
			return new Promise(function(r, a) {
				n = e[t](n), i(r, a, n.done, n.value);
			});
		};
	}
	function i(e, t, n, r) {
		Promise.resolve(r).then(function(t) {
			e({
				value: t,
				done: n
			});
		}, t);
	}
}
//#endregion
//#region node_modules/rxjs/dist/esm5/internal/util/isFunction.js
function z(e) {
	return typeof e == "function";
}
//#endregion
//#region node_modules/rxjs/dist/esm5/internal/util/createErrorClass.js
function xr(e) {
	var t = e(function(e) {
		Error.call(e), e.stack = (/* @__PURE__ */ Error()).stack;
	});
	return t.prototype = Object.create(Error.prototype), t.prototype.constructor = t, t;
}
//#endregion
//#region node_modules/rxjs/dist/esm5/internal/util/UnsubscriptionError.js
var Sr = xr(function(e) {
	return function(t) {
		e(this), this.message = t ? t.length + " errors occurred during unsubscription:\n" + t.map(function(e, t) {
			return t + 1 + ") " + e.toString();
		}).join("\n  ") : "", this.name = "UnsubscriptionError", this.errors = t;
	};
});
//#endregion
//#region node_modules/rxjs/dist/esm5/internal/util/arrRemove.js
function Cr(e, t) {
	if (e) {
		var n = e.indexOf(t);
		0 <= n && e.splice(n, 1);
	}
}
//#endregion
//#region node_modules/rxjs/dist/esm5/internal/Subscription.js
var wr = function() {
	function e(e) {
		this.initialTeardown = e, this.closed = !1, this._parentage = null, this._finalizers = null;
	}
	return e.prototype.unsubscribe = function() {
		var e, t, n, r, i;
		if (!this.closed) {
			this.closed = !0;
			var a = this._parentage;
			if (a) if (this._parentage = null, Array.isArray(a)) try {
				for (var o = hr(a), s = o.next(); !s.done; s = o.next()) s.value.remove(this);
			} catch (t) {
				e = { error: t };
			} finally {
				try {
					s && !s.done && (t = o.return) && t.call(o);
				} finally {
					if (e) throw e.error;
				}
			}
			else a.remove(this);
			var c = this.initialTeardown;
			if (z(c)) try {
				c();
			} catch (e) {
				i = e instanceof Sr ? e.errors : [e];
			}
			var l = this._finalizers;
			if (l) {
				this._finalizers = null;
				try {
					for (var u = hr(l), d = u.next(); !d.done; d = u.next()) {
						var f = d.value;
						try {
							Dr(f);
						} catch (e) {
							i ??= [], e instanceof Sr ? i = _r(_r([], gr(i)), gr(e.errors)) : i.push(e);
						}
					}
				} catch (e) {
					n = { error: e };
				} finally {
					try {
						d && !d.done && (r = u.return) && r.call(u);
					} finally {
						if (n) throw n.error;
					}
				}
			}
			if (i) throw new Sr(i);
		}
	}, e.prototype.add = function(t) {
		if (t && t !== this) if (this.closed) Dr(t);
		else {
			if (t instanceof e) {
				if (t.closed || t._hasParent(this)) return;
				t._addParent(this);
			}
			(this._finalizers = this._finalizers ?? []).push(t);
		}
	}, e.prototype._hasParent = function(e) {
		var t = this._parentage;
		return t === e || Array.isArray(t) && t.includes(e);
	}, e.prototype._addParent = function(e) {
		var t = this._parentage;
		this._parentage = Array.isArray(t) ? (t.push(e), t) : t ? [t, e] : e;
	}, e.prototype._removeParent = function(e) {
		var t = this._parentage;
		t === e ? this._parentage = null : Array.isArray(t) && Cr(t, e);
	}, e.prototype.remove = function(t) {
		var n = this._finalizers;
		n && Cr(n, t), t instanceof e && t._removeParent(this);
	}, e.EMPTY = (function() {
		var t = new e();
		return t.closed = !0, t;
	})(), e;
}(), Tr = wr.EMPTY;
function Er(e) {
	return e instanceof wr || e && "closed" in e && z(e.remove) && z(e.add) && z(e.unsubscribe);
}
function Dr(e) {
	z(e) ? e() : e.unsubscribe();
}
//#endregion
//#region node_modules/rxjs/dist/esm5/internal/config.js
var Or = {
	onUnhandledError: null,
	onStoppedNotification: null,
	Promise: void 0,
	useDeprecatedSynchronousErrorHandling: !1,
	useDeprecatedNextContext: !1
}, kr = {
	setTimeout: function(e, t) {
		var n = [...arguments].slice(2), r = kr.delegate;
		return r?.setTimeout ? r.setTimeout.apply(r, _r([e, t], gr(n))) : setTimeout.apply(void 0, _r([e, t], gr(n)));
	},
	clearTimeout: function(e) {
		return (kr.delegate?.clearTimeout || clearTimeout)(e);
	},
	delegate: void 0
};
//#endregion
//#region node_modules/rxjs/dist/esm5/internal/util/reportUnhandledError.js
function Ar(e) {
	kr.setTimeout(function() {
		var t = Or.onUnhandledError;
		if (t) t(e);
		else throw e;
	});
}
//#endregion
//#region node_modules/rxjs/dist/esm5/internal/util/noop.js
function jr() {}
//#endregion
//#region node_modules/rxjs/dist/esm5/internal/NotificationFactories.js
var Mr = (function() {
	return Fr("C", void 0, void 0);
})();
function Nr(e) {
	return Fr("E", void 0, e);
}
function Pr(e) {
	return Fr("N", e, void 0);
}
function Fr(e, t, n) {
	return {
		kind: e,
		value: t,
		error: n
	};
}
//#endregion
//#region node_modules/rxjs/dist/esm5/internal/util/errorContext.js
var Ir = null;
function Lr(e) {
	if (Or.useDeprecatedSynchronousErrorHandling) {
		var t = !Ir;
		if (t && (Ir = {
			errorThrown: !1,
			error: null
		}), e(), t) {
			var n = Ir, r = n.errorThrown, i = n.error;
			if (Ir = null, r) throw i;
		}
	} else e();
}
function Rr(e) {
	Or.useDeprecatedSynchronousErrorHandling && Ir && (Ir.errorThrown = !0, Ir.error = e);
}
//#endregion
//#region node_modules/rxjs/dist/esm5/internal/Subscriber.js
var zr = function(e) {
	fr(t, e);
	function t(t) {
		var n = e.call(this) || this;
		return n.isStopped = !1, t ? (n.destination = t, Er(t) && t.add(n)) : n.destination = qr, n;
	}
	return t.create = function(e, t, n) {
		return new Ur(e, t, n);
	}, t.prototype.next = function(e) {
		this.isStopped ? Kr(Pr(e), this) : this._next(e);
	}, t.prototype.error = function(e) {
		this.isStopped ? Kr(Nr(e), this) : (this.isStopped = !0, this._error(e));
	}, t.prototype.complete = function() {
		this.isStopped ? Kr(Mr, this) : (this.isStopped = !0, this._complete());
	}, t.prototype.unsubscribe = function() {
		this.closed || (this.isStopped = !0, e.prototype.unsubscribe.call(this), this.destination = null);
	}, t.prototype._next = function(e) {
		this.destination.next(e);
	}, t.prototype._error = function(e) {
		try {
			this.destination.error(e);
		} finally {
			this.unsubscribe();
		}
	}, t.prototype._complete = function() {
		try {
			this.destination.complete();
		} finally {
			this.unsubscribe();
		}
	}, t;
}(wr), Br = Function.prototype.bind;
function Vr(e, t) {
	return Br.call(e, t);
}
var Hr = function() {
	function e(e) {
		this.partialObserver = e;
	}
	return e.prototype.next = function(e) {
		var t = this.partialObserver;
		if (t.next) try {
			t.next(e);
		} catch (e) {
			Wr(e);
		}
	}, e.prototype.error = function(e) {
		var t = this.partialObserver;
		if (t.error) try {
			t.error(e);
		} catch (e) {
			Wr(e);
		}
		else Wr(e);
	}, e.prototype.complete = function() {
		var e = this.partialObserver;
		if (e.complete) try {
			e.complete();
		} catch (e) {
			Wr(e);
		}
	}, e;
}(), Ur = function(e) {
	fr(t, e);
	function t(t, n, r) {
		var i = e.call(this) || this, a;
		if (z(t) || !t) a = {
			next: t ?? void 0,
			error: n ?? void 0,
			complete: r ?? void 0
		};
		else {
			var o;
			i && Or.useDeprecatedNextContext ? (o = Object.create(t), o.unsubscribe = function() {
				return i.unsubscribe();
			}, a = {
				next: t.next && Vr(t.next, o),
				error: t.error && Vr(t.error, o),
				complete: t.complete && Vr(t.complete, o)
			}) : a = t;
		}
		return i.destination = new Hr(a), i;
	}
	return t;
}(zr);
function Wr(e) {
	Or.useDeprecatedSynchronousErrorHandling ? Rr(e) : Ar(e);
}
function Gr(e) {
	throw e;
}
function Kr(e, t) {
	var n = Or.onStoppedNotification;
	n && kr.setTimeout(function() {
		return n(e, t);
	});
}
var qr = {
	closed: !0,
	next: jr,
	error: Gr,
	complete: jr
}, Jr = (function() {
	return typeof Symbol == "function" && Symbol.observable || "@@observable";
})();
//#endregion
//#region node_modules/rxjs/dist/esm5/internal/util/identity.js
function Yr(e) {
	return e;
}
//#endregion
//#region node_modules/rxjs/dist/esm5/internal/util/pipe.js
function Xr() {
	return Zr([...arguments]);
}
function Zr(e) {
	return e.length === 0 ? Yr : e.length === 1 ? e[0] : function(t) {
		return e.reduce(function(e, t) {
			return t(e);
		}, t);
	};
}
//#endregion
//#region node_modules/rxjs/dist/esm5/internal/Observable.js
var B = function() {
	function e(e) {
		e && (this._subscribe = e);
	}
	return e.prototype.lift = function(t) {
		var n = new e();
		return n.source = this, n.operator = t, n;
	}, e.prototype.subscribe = function(e, t, n) {
		var r = this, i = ei(e) ? e : new Ur(e, t, n);
		return Lr(function() {
			var e = r, t = e.operator, n = e.source;
			i.add(t ? t.call(i, n) : n ? r._subscribe(i) : r._trySubscribe(i));
		}), i;
	}, e.prototype._trySubscribe = function(e) {
		try {
			return this._subscribe(e);
		} catch (t) {
			e.error(t);
		}
	}, e.prototype.forEach = function(e, t) {
		var n = this;
		return t = Qr(t), new t(function(t, r) {
			var i = new Ur({
				next: function(t) {
					try {
						e(t);
					} catch (e) {
						r(e), i.unsubscribe();
					}
				},
				error: r,
				complete: t
			});
			n.subscribe(i);
		});
	}, e.prototype._subscribe = function(e) {
		return this.source?.subscribe(e);
	}, e.prototype[Jr] = function() {
		return this;
	}, e.prototype.pipe = function() {
		return Zr([...arguments])(this);
	}, e.prototype.toPromise = function(e) {
		var t = this;
		return e = Qr(e), new e(function(e, n) {
			var r;
			t.subscribe(function(e) {
				return r = e;
			}, function(e) {
				return n(e);
			}, function() {
				return e(r);
			});
		});
	}, e.create = function(t) {
		return new e(t);
	}, e;
}();
function Qr(e) {
	return e ?? Or.Promise ?? Promise;
}
function $r(e) {
	return e && z(e.next) && z(e.error) && z(e.complete);
}
function ei(e) {
	return e && e instanceof zr || $r(e) && Er(e);
}
//#endregion
//#region node_modules/rxjs/dist/esm5/internal/util/lift.js
function ti(e) {
	return z(e?.lift);
}
function V(e) {
	return function(t) {
		if (ti(t)) return t.lift(function(t) {
			try {
				return e(t, this);
			} catch (e) {
				this.error(e);
			}
		});
		throw TypeError("Unable to lift unknown Observable type");
	};
}
//#endregion
//#region node_modules/rxjs/dist/esm5/internal/operators/OperatorSubscriber.js
function H(e, t, n, r, i) {
	return new ni(e, t, n, r, i);
}
var ni = function(e) {
	fr(t, e);
	function t(t, n, r, i, a, o) {
		var s = e.call(this, t) || this;
		return s.onFinalize = a, s.shouldUnsubscribe = o, s._next = n ? function(e) {
			try {
				n(e);
			} catch (e) {
				t.error(e);
			}
		} : e.prototype._next, s._error = i ? function(e) {
			try {
				i(e);
			} catch (e) {
				t.error(e);
			} finally {
				this.unsubscribe();
			}
		} : e.prototype._error, s._complete = r ? function() {
			try {
				r();
			} catch (e) {
				t.error(e);
			} finally {
				this.unsubscribe();
			}
		} : e.prototype._complete, s;
	}
	return t.prototype.unsubscribe = function() {
		var t;
		if (!this.shouldUnsubscribe || this.shouldUnsubscribe()) {
			var n = this.closed;
			e.prototype.unsubscribe.call(this), !n && ((t = this.onFinalize) == null || t.call(this));
		}
	}, t;
}(zr), ri = xr(function(e) {
	return function() {
		e(this), this.name = "ObjectUnsubscribedError", this.message = "object unsubscribed";
	};
}), ii = function(e) {
	fr(t, e);
	function t() {
		var t = e.call(this) || this;
		return t.closed = !1, t.currentObservers = null, t.observers = [], t.isStopped = !1, t.hasError = !1, t.thrownError = null, t;
	}
	return t.prototype.lift = function(e) {
		var t = new ai(this, this);
		return t.operator = e, t;
	}, t.prototype._throwIfClosed = function() {
		if (this.closed) throw new ri();
	}, t.prototype.next = function(e) {
		var t = this;
		Lr(function() {
			var n, r;
			if (t._throwIfClosed(), !t.isStopped) {
				t.currentObservers ||= Array.from(t.observers);
				try {
					for (var i = hr(t.currentObservers), a = i.next(); !a.done; a = i.next()) a.value.next(e);
				} catch (e) {
					n = { error: e };
				} finally {
					try {
						a && !a.done && (r = i.return) && r.call(i);
					} finally {
						if (n) throw n.error;
					}
				}
			}
		});
	}, t.prototype.error = function(e) {
		var t = this;
		Lr(function() {
			if (t._throwIfClosed(), !t.isStopped) {
				t.hasError = t.isStopped = !0, t.thrownError = e;
				for (var n = t.observers; n.length;) n.shift().error(e);
			}
		});
	}, t.prototype.complete = function() {
		var e = this;
		Lr(function() {
			if (e._throwIfClosed(), !e.isStopped) {
				e.isStopped = !0;
				for (var t = e.observers; t.length;) t.shift().complete();
			}
		});
	}, t.prototype.unsubscribe = function() {
		this.isStopped = this.closed = !0, this.observers = this.currentObservers = null;
	}, Object.defineProperty(t.prototype, "observed", {
		get: function() {
			return this.observers?.length > 0;
		},
		enumerable: !1,
		configurable: !0
	}), t.prototype._trySubscribe = function(t) {
		return this._throwIfClosed(), e.prototype._trySubscribe.call(this, t);
	}, t.prototype._subscribe = function(e) {
		return this._throwIfClosed(), this._checkFinalizedStatuses(e), this._innerSubscribe(e);
	}, t.prototype._innerSubscribe = function(e) {
		var t = this, n = this, r = n.hasError, i = n.isStopped, a = n.observers;
		return r || i ? Tr : (this.currentObservers = null, a.push(e), new wr(function() {
			t.currentObservers = null, Cr(a, e);
		}));
	}, t.prototype._checkFinalizedStatuses = function(e) {
		var t = this, n = t.hasError, r = t.thrownError, i = t.isStopped;
		n ? e.error(r) : i && e.complete();
	}, t.prototype.asObservable = function() {
		var e = new B();
		return e.source = this, e;
	}, t.create = function(e, t) {
		return new ai(e, t);
	}, t;
}(B), ai = function(e) {
	fr(t, e);
	function t(t, n) {
		var r = e.call(this) || this;
		return r.destination = t, r.source = n, r;
	}
	return t.prototype.next = function(e) {
		var t, n;
		(n = (t = this.destination)?.next) == null || n.call(t, e);
	}, t.prototype.error = function(e) {
		var t, n;
		(n = (t = this.destination)?.error) == null || n.call(t, e);
	}, t.prototype.complete = function() {
		var e, t;
		(t = (e = this.destination)?.complete) == null || t.call(e);
	}, t.prototype._subscribe = function(e) {
		return this.source?.subscribe(e) ?? Tr;
	}, t;
}(ii), oi = {
	now: function() {
		return (oi.delegate || Date).now();
	},
	delegate: void 0
}, si = function(e) {
	fr(t, e);
	function t(t, n, r) {
		t === void 0 && (t = Infinity), n === void 0 && (n = Infinity), r === void 0 && (r = oi);
		var i = e.call(this) || this;
		return i._bufferSize = t, i._windowTime = n, i._timestampProvider = r, i._buffer = [], i._infiniteTimeWindow = !0, i._infiniteTimeWindow = n === Infinity, i._bufferSize = Math.max(1, t), i._windowTime = Math.max(1, n), i;
	}
	return t.prototype.next = function(t) {
		var n = this, r = n.isStopped, i = n._buffer, a = n._infiniteTimeWindow, o = n._timestampProvider, s = n._windowTime;
		r || (i.push(t), !a && i.push(o.now() + s)), this._trimBuffer(), e.prototype.next.call(this, t);
	}, t.prototype._subscribe = function(e) {
		this._throwIfClosed(), this._trimBuffer();
		for (var t = this._innerSubscribe(e), n = this, r = n._infiniteTimeWindow, i = n._buffer.slice(), a = 0; a < i.length && !e.closed; a += r ? 1 : 2) e.next(i[a]);
		return this._checkFinalizedStatuses(e), t;
	}, t.prototype._trimBuffer = function() {
		var e = this, t = e._bufferSize, n = e._timestampProvider, r = e._buffer, i = e._infiniteTimeWindow, a = (i ? 1 : 2) * t;
		if (t < Infinity && a < r.length && r.splice(0, r.length - a), !i) {
			for (var o = n.now(), s = 0, c = 1; c < r.length && r[c] <= o; c += 2) s = c;
			s && r.splice(0, s + 1);
		}
	}, t;
}(ii), ci = new B(function(e) {
	return e.complete();
});
//#endregion
//#region node_modules/rxjs/dist/esm5/internal/util/isScheduler.js
function li(e) {
	return e && z(e.schedule);
}
//#endregion
//#region node_modules/rxjs/dist/esm5/internal/util/args.js
function ui(e) {
	return e[e.length - 1];
}
function di(e) {
	return li(ui(e)) ? e.pop() : void 0;
}
//#endregion
//#region node_modules/rxjs/dist/esm5/internal/util/isArrayLike.js
var fi = (function(e) {
	return e && typeof e.length == "number" && typeof e != "function";
});
//#endregion
//#region node_modules/rxjs/dist/esm5/internal/util/isPromise.js
function pi(e) {
	return z(e?.then);
}
//#endregion
//#region node_modules/rxjs/dist/esm5/internal/util/isInteropObservable.js
function mi(e) {
	return z(e[Jr]);
}
//#endregion
//#region node_modules/rxjs/dist/esm5/internal/util/isAsyncIterable.js
function hi(e) {
	return Symbol.asyncIterator && z(e?.[Symbol.asyncIterator]);
}
//#endregion
//#region node_modules/rxjs/dist/esm5/internal/util/throwUnobservableError.js
function gi(e) {
	return /* @__PURE__ */ TypeError("You provided " + (typeof e == "object" && e ? "an invalid object" : "'" + e + "'") + " where a stream was expected. You can provide an Observable, Promise, ReadableStream, Array, AsyncIterable, or Iterable.");
}
//#endregion
//#region node_modules/rxjs/dist/esm5/internal/symbol/iterator.js
function _i() {
	return typeof Symbol != "function" || !Symbol.iterator ? "@@iterator" : Symbol.iterator;
}
var vi = _i();
//#endregion
//#region node_modules/rxjs/dist/esm5/internal/util/isIterable.js
function yi(e) {
	return z(e?.[vi]);
}
//#endregion
//#region node_modules/rxjs/dist/esm5/internal/util/isReadableStreamLike.js
function bi(e) {
	return yr(this, arguments, function() {
		var t, n, r, i;
		return mr(this, function(a) {
			switch (a.label) {
				case 0: t = e.getReader(), a.label = 1;
				case 1: a.trys.push([
					1,
					,
					9,
					10
				]), a.label = 2;
				case 2: return [4, vr(t.read())];
				case 3: return n = a.sent(), r = n.value, i = n.done, i ? [4, vr(void 0)] : [3, 5];
				case 4: return [2, a.sent()];
				case 5: return [4, vr(r)];
				case 6: return [4, a.sent()];
				case 7: return a.sent(), [3, 2];
				case 8: return [3, 10];
				case 9: return t.releaseLock(), [7];
				case 10: return [2];
			}
		});
	});
}
function xi(e) {
	return z(e?.getReader);
}
//#endregion
//#region node_modules/rxjs/dist/esm5/internal/observable/innerFrom.js
function Si(e) {
	if (e instanceof B) return e;
	if (e != null) {
		if (mi(e)) return Ci(e);
		if (fi(e)) return wi(e);
		if (pi(e)) return Ti(e);
		if (hi(e)) return Di(e);
		if (yi(e)) return Ei(e);
		if (xi(e)) return Oi(e);
	}
	throw gi(e);
}
function Ci(e) {
	return new B(function(t) {
		var n = e[Jr]();
		if (z(n.subscribe)) return n.subscribe(t);
		throw TypeError("Provided object does not correctly implement Symbol.observable");
	});
}
function wi(e) {
	return new B(function(t) {
		for (var n = 0; n < e.length && !t.closed; n++) t.next(e[n]);
		t.complete();
	});
}
function Ti(e) {
	return new B(function(t) {
		e.then(function(e) {
			t.closed || (t.next(e), t.complete());
		}, function(e) {
			return t.error(e);
		}).then(null, Ar);
	});
}
function Ei(e) {
	return new B(function(t) {
		var n, r;
		try {
			for (var i = hr(e), a = i.next(); !a.done; a = i.next()) {
				var o = a.value;
				if (t.next(o), t.closed) return;
			}
		} catch (e) {
			n = { error: e };
		} finally {
			try {
				a && !a.done && (r = i.return) && r.call(i);
			} finally {
				if (n) throw n.error;
			}
		}
		t.complete();
	});
}
function Di(e) {
	return new B(function(t) {
		ki(e, t).catch(function(e) {
			return t.error(e);
		});
	});
}
function Oi(e) {
	return Di(bi(e));
}
function ki(e, t) {
	var n, r, i, a;
	return pr(this, void 0, void 0, function() {
		var o, s;
		return mr(this, function(c) {
			switch (c.label) {
				case 0: c.trys.push([
					0,
					5,
					6,
					11
				]), n = br(e), c.label = 1;
				case 1: return [4, n.next()];
				case 2:
					if (r = c.sent(), r.done) return [3, 4];
					if (o = r.value, t.next(o), t.closed) return [2];
					c.label = 3;
				case 3: return [3, 1];
				case 4: return [3, 11];
				case 5: return s = c.sent(), i = { error: s }, [3, 11];
				case 6: return c.trys.push([
					6,
					,
					9,
					10
				]), r && !r.done && (a = n.return) ? [4, a.call(n)] : [3, 8];
				case 7: c.sent(), c.label = 8;
				case 8: return [3, 10];
				case 9:
					if (i) throw i.error;
					return [7];
				case 10: return [7];
				case 11: return t.complete(), [2];
			}
		});
	});
}
//#endregion
//#region node_modules/rxjs/dist/esm5/internal/util/executeSchedule.js
function Ai(e, t, n, r, i) {
	r === void 0 && (r = 0), i === void 0 && (i = !1);
	var a = t.schedule(function() {
		n(), i ? e.add(this.schedule(null, r)) : this.unsubscribe();
	}, r);
	if (e.add(a), !i) return a;
}
//#endregion
//#region node_modules/rxjs/dist/esm5/internal/operators/observeOn.js
function ji(e, t) {
	return t === void 0 && (t = 0), V(function(n, r) {
		n.subscribe(H(r, function(n) {
			return Ai(r, e, function() {
				return r.next(n);
			}, t);
		}, function() {
			return Ai(r, e, function() {
				return r.complete();
			}, t);
		}, function(n) {
			return Ai(r, e, function() {
				return r.error(n);
			}, t);
		}));
	});
}
//#endregion
//#region node_modules/rxjs/dist/esm5/internal/operators/subscribeOn.js
function Mi(e, t) {
	return t === void 0 && (t = 0), V(function(n, r) {
		r.add(e.schedule(function() {
			return n.subscribe(r);
		}, t));
	});
}
//#endregion
//#region node_modules/rxjs/dist/esm5/internal/scheduled/scheduleObservable.js
function Ni(e, t) {
	return Si(e).pipe(Mi(t), ji(t));
}
//#endregion
//#region node_modules/rxjs/dist/esm5/internal/scheduled/schedulePromise.js
function Pi(e, t) {
	return Si(e).pipe(Mi(t), ji(t));
}
//#endregion
//#region node_modules/rxjs/dist/esm5/internal/scheduled/scheduleArray.js
function Fi(e, t) {
	return new B(function(n) {
		var r = 0;
		return t.schedule(function() {
			r === e.length ? n.complete() : (n.next(e[r++]), n.closed || this.schedule());
		});
	});
}
//#endregion
//#region node_modules/rxjs/dist/esm5/internal/scheduled/scheduleIterable.js
function Ii(e, t) {
	return new B(function(n) {
		var r;
		return Ai(n, t, function() {
			r = e[vi](), Ai(n, t, function() {
				var e, t, i;
				try {
					e = r.next(), t = e.value, i = e.done;
				} catch (e) {
					n.error(e);
					return;
				}
				i ? n.complete() : n.next(t);
			}, 0, !0);
		}), function() {
			return z(r?.return) && r.return();
		};
	});
}
//#endregion
//#region node_modules/rxjs/dist/esm5/internal/scheduled/scheduleAsyncIterable.js
function Li(e, t) {
	if (!e) throw Error("Iterable cannot be null");
	return new B(function(n) {
		Ai(n, t, function() {
			var r = e[Symbol.asyncIterator]();
			Ai(n, t, function() {
				r.next().then(function(e) {
					e.done ? n.complete() : n.next(e.value);
				});
			}, 0, !0);
		});
	});
}
//#endregion
//#region node_modules/rxjs/dist/esm5/internal/scheduled/scheduleReadableStreamLike.js
function Ri(e, t) {
	return Li(bi(e), t);
}
//#endregion
//#region node_modules/rxjs/dist/esm5/internal/scheduled/scheduled.js
function zi(e, t) {
	if (e != null) {
		if (mi(e)) return Ni(e, t);
		if (fi(e)) return Fi(e, t);
		if (pi(e)) return Pi(e, t);
		if (hi(e)) return Li(e, t);
		if (yi(e)) return Ii(e, t);
		if (xi(e)) return Ri(e, t);
	}
	throw gi(e);
}
//#endregion
//#region node_modules/rxjs/dist/esm5/internal/observable/from.js
function Bi(e, t) {
	return t ? zi(e, t) : Si(e);
}
//#endregion
//#region node_modules/rxjs/dist/esm5/internal/observable/of.js
function U() {
	var e = [...arguments];
	return Bi(e, di(e));
}
//#endregion
//#region node_modules/rxjs/dist/esm5/internal/observable/throwError.js
function W(e, t) {
	var n = z(e) ? e : function() {
		return e;
	}, r = function(e) {
		return e.error(n());
	};
	return new B(t ? function(e) {
		return t.schedule(r, 0, e);
	} : r);
}
//#endregion
//#region node_modules/rxjs/dist/esm5/internal/util/EmptyError.js
var Vi = xr(function(e) {
	return function() {
		e(this), this.name = "EmptyError", this.message = "no elements in sequence";
	};
});
//#endregion
//#region node_modules/rxjs/dist/esm5/internal/lastValueFrom.js
function Hi(e, t) {
	var n = typeof t == "object";
	return new Promise(function(r, i) {
		var a = !1, o;
		e.subscribe({
			next: function(e) {
				o = e, a = !0;
			},
			error: i,
			complete: function() {
				a ? r(o) : n ? r(t.defaultValue) : i(new Vi());
			}
		});
	});
}
//#endregion
//#region node_modules/rxjs/dist/esm5/internal/operators/map.js
function Ui(e, t) {
	return V(function(n, r) {
		var i = 0;
		n.subscribe(H(r, function(n) {
			r.next(e.call(t, n, i++));
		}));
	});
}
//#endregion
//#region node_modules/rxjs/dist/esm5/internal/operators/mergeInternals.js
function Wi(e, t, n, r, i, a, o, s) {
	var c = [], l = 0, u = 0, d = !1, f = function() {
		d && !c.length && !l && t.complete();
	}, p = function(e) {
		return l < r ? m(e) : c.push(e);
	}, m = function(e) {
		a && t.next(e), l++;
		var s = !1;
		Si(n(e, u++)).subscribe(H(t, function(e) {
			i?.(e), a ? p(e) : t.next(e);
		}, function() {
			s = !0;
		}, void 0, function() {
			if (s) try {
				l--;
				for (var e = function() {
					var e = c.shift();
					o ? Ai(t, o, function() {
						return m(e);
					}) : m(e);
				}; c.length && l < r;) e();
				f();
			} catch (e) {
				t.error(e);
			}
		}));
	};
	return e.subscribe(H(t, p, function() {
		d = !0, f();
	})), function() {
		s?.();
	};
}
//#endregion
//#region node_modules/rxjs/dist/esm5/internal/operators/mergeMap.js
function Gi(e, t, n) {
	return n === void 0 && (n = Infinity), z(t) ? Gi(function(n, r) {
		return Ui(function(e, i) {
			return t(n, e, r, i);
		})(Si(e(n, r)));
	}, n) : (typeof t == "number" && (n = t), V(function(t, r) {
		return Wi(t, r, e, n);
	}));
}
//#endregion
//#region node_modules/rxjs/dist/esm5/internal/operators/mergeAll.js
function Ki(e) {
	return e === void 0 && (e = Infinity), Gi(Yr, e);
}
//#endregion
//#region node_modules/rxjs/dist/esm5/internal/observable/defer.js
function qi(e) {
	return new B(function(t) {
		Si(e()).subscribe(t);
	});
}
//#endregion
//#region node_modules/rxjs/dist/esm5/internal/operators/catchError.js
function Ji(e) {
	return V(function(t, n) {
		var r = null, i = !1, a;
		r = t.subscribe(H(n, void 0, void 0, function(o) {
			a = Si(e(o, Ji(e)(t))), r ? (r.unsubscribe(), r = null, a.subscribe(n)) : i = !0;
		})), i && (r.unsubscribe(), r = null, a.subscribe(n));
	});
}
//#endregion
//#region node_modules/rxjs/dist/esm5/internal/operators/concatMap.js
function Yi(e, t) {
	return z(t) ? Gi(e, t, 1) : Gi(e, 1);
}
//#endregion
//#region node_modules/rxjs/dist/esm5/internal/operators/defaultIfEmpty.js
function Xi(e) {
	return V(function(t, n) {
		var r = !1;
		t.subscribe(H(n, function(e) {
			r = !0, n.next(e);
		}, function() {
			r || n.next(e), n.complete();
		}));
	});
}
//#endregion
//#region node_modules/rxjs/dist/esm5/internal/operators/finalize.js
function Zi(e) {
	return V(function(t, n) {
		try {
			t.subscribe(n);
		} finally {
			n.add(e);
		}
	});
}
//#endregion
//#region node_modules/rxjs/dist/esm5/internal/operators/switchMap.js
function Qi(e, t) {
	return V(function(n, r) {
		var i = null, a = 0, o = !1, s = function() {
			return o && !i && r.complete();
		};
		n.subscribe(H(r, function(n) {
			i?.unsubscribe();
			var o = 0, c = a++;
			Si(e(n, c)).subscribe(i = H(r, function(e) {
				return r.next(t ? t(n, e, c, o++) : e);
			}, function() {
				i = null, s();
			}));
		}, function() {
			o = !0, s();
		}));
	});
}
//#endregion
//#region node_modules/rxjs/dist/esm5/internal/operators/takeUntil.js
function $i(e) {
	return V(function(t, n) {
		Si(e).subscribe(H(n, function() {
			return n.complete();
		}, jr)), !n.closed && t.subscribe(n);
	});
}
//#endregion
//#region node_modules/rxjs/dist/esm5/internal/operators/tap.js
function ea(e, t, n) {
	var r = z(e) || t || n ? {
		next: e,
		error: t,
		complete: n
	} : e;
	return r ? V(function(e, t) {
		var n;
		(n = r.subscribe) == null || n.call(r);
		var i = !0;
		e.subscribe(H(t, function(e) {
			var n;
			(n = r.next) == null || n.call(r, e), t.next(e);
		}, function() {
			var e;
			i = !1, (e = r.complete) == null || e.call(r), t.complete();
		}, function(e) {
			var n;
			i = !1, (n = r.error) == null || n.call(r, e), t.error(e);
		}, function() {
			var e, t;
			i && ((e = r.unsubscribe) == null || e.call(r)), (t = r.finalize) == null || t.call(r);
		}));
	}) : Yr;
}
//#endregion
//#region node_modules/untruncate-json/dist/esm/index.js
function ta(e) {
	return " \r\n	".indexOf(e) >= 0;
}
function na(e) {
	for (var t = ["topLevel"], n = 0, r, i, a, o = function(e) {
		return t.push(e);
	}, s = function(e) {
		return t[t.length - 1] = e;
	}, c = function(e) {
		r ?? (r = n, i = t.length, a = e);
	}, l = function(e) {
		e === a && (r = void 0, i = void 0, a = void 0);
	}, u = function() {
		return t.pop();
	}, d = function() {
		return n--;
	}, f = function(e) {
		if ("0" <= e && e <= "9") {
			o("number");
			return;
		}
		switch (e) {
			case "\"":
				o("string");
				return;
			case "-":
				o("numberNeedsDigit");
				return;
			case "t":
				o("true");
				return;
			case "f":
				o("false");
				return;
			case "n":
				o("null");
				return;
			case "[":
				o("arrayNeedsValue");
				return;
			case "{":
				o("objectNeedsKey");
				return;
		}
	}, p = e.length; n < p; n++) {
		var m = e[n];
		switch (t[t.length - 1]) {
			case "topLevel":
				f(m);
				break;
			case "string":
				switch (m) {
					case "\"":
						u();
						break;
					case "\\":
						c("stringEscape"), o("stringEscaped");
						break;
				}
				break;
			case "stringEscaped":
				m === "u" ? o("stringUnicode") : (l("stringEscape"), u());
				break;
			case "stringUnicode":
				n - e.lastIndexOf("u", n) === 4 && (l("stringEscape"), u());
				break;
			case "number":
				m === "." ? s("numberNeedsDigit") : m === "e" || m === "E" ? s("numberNeedsExponent") : (m < "0" || m > "9") && (d(), u());
				break;
			case "numberNeedsDigit":
				s("number");
				break;
			case "numberNeedsExponent":
				s(m === "+" || m === "-" ? "numberNeedsDigit" : "number");
				break;
			case "true":
			case "false":
			case "null":
				(m < "a" || m > "z") && (d(), u());
				break;
			case "arrayNeedsValue":
				m === "]" ? u() : ta(m) || (l("collectionItem"), s("arrayNeedsComma"), f(m));
				break;
			case "arrayNeedsComma":
				m === "]" ? u() : m === "," && (c("collectionItem"), s("arrayNeedsValue"));
				break;
			case "objectNeedsKey":
				m === "}" ? u() : m === "\"" && (c("collectionItem"), s("objectNeedsColon"), o("string"));
				break;
			case "objectNeedsColon":
				m === ":" && s("objectNeedsValue");
				break;
			case "objectNeedsValue":
				ta(m) || (l("collectionItem"), s("objectNeedsComma"), f(m));
				break;
			case "objectNeedsComma":
				m === "}" ? u() : m === "," && (c("collectionItem"), s("objectNeedsKey"));
				break;
		}
	}
	i != null && (t.length = i);
	for (var h = [r == null ? e : e.slice(0, r)], ee = function(t) {
		return h.push(t.slice(e.length - e.lastIndexOf(t[0])));
	}, te = t.length - 1; te >= 0; te--) switch (t[te]) {
		case "string":
			h.push("\"");
			break;
		case "numberNeedsDigit":
		case "numberNeedsExponent":
			h.push("0");
			break;
		case "true":
			ee("true");
			break;
		case "false":
			ee("false");
			break;
		case "null":
			ee("null");
			break;
		case "arrayNeedsValue":
		case "arrayNeedsComma":
			h.push("]");
			break;
		case "objectNeedsKey":
		case "objectNeedsColon":
		case "objectNeedsValue":
		case "objectNeedsComma":
			h.push("}");
			break;
	}
	return h.join("");
}
//#endregion
//#region node_modules/@bufbuild/protobuf/dist/esm/wire/varint.js
function ra() {
	let e = 0, t = 0;
	for (let n = 0; n < 28; n += 7) {
		let r = this.buf[this.pos++];
		if (e |= (r & 127) << n, !(r & 128)) return this.assertBounds(), [e, t];
	}
	let n = this.buf[this.pos++];
	if (e |= (n & 15) << 28, t = (n & 112) >> 4, !(n & 128)) return this.assertBounds(), [e, t];
	for (let n = 3; n <= 31; n += 7) {
		let r = this.buf[this.pos++];
		if (t |= (r & 127) << n, !(r & 128)) return this.assertBounds(), [e, t];
	}
	throw Error("invalid varint");
}
function ia(e, t, n) {
	for (let r = 0; r < 28; r += 7) {
		let i = e >>> r, a = !(!(i >>> 7) && t == 0), o = (a ? i | 128 : i) & 255;
		if (n.push(o), !a) return;
	}
	let r = e >>> 28 & 15 | (t & 7) << 4, i = !!(t >> 3);
	if (n.push((i ? r | 128 : r) & 255), i) {
		for (let e = 3; e < 31; e += 7) {
			let r = t >>> e, i = !!(r >>> 7), a = (i ? r | 128 : r) & 255;
			if (n.push(a), !i) return;
		}
		n.push(t >>> 31 & 1);
	}
}
var aa = 4294967296;
function oa(e) {
	let t = e[0] === "-";
	t && (e = e.slice(1));
	let n = 1e6, r = 0, i = 0;
	function a(t, a) {
		let o = Number(e.slice(t, a));
		i *= n, r = r * n + o, r >= aa && (i += r / aa | 0, r %= aa);
	}
	return a(-24, -18), a(-18, -12), a(-12, -6), a(-6), t ? da(r, i) : ua(r, i);
}
function sa(e, t) {
	let n = ua(e, t), r = n.hi & 2147483648;
	r && (n = da(n.lo, n.hi));
	let i = ca(n.lo, n.hi);
	return r ? "-" + i : i;
}
function ca(e, t) {
	if ({lo: e, hi: t} = la(e, t), t <= 2097151) return String(aa * t + e);
	let n = e & 16777215, r = (e >>> 24 | t << 8) & 16777215, i = t >> 16 & 65535, a = n + r * 6777216 + i * 6710656, o = r + i * 8147497, s = i * 2, c = 1e7;
	return a >= c && (o += Math.floor(a / c), a %= c), o >= c && (s += Math.floor(o / c), o %= c), s.toString() + fa(o) + fa(a);
}
function la(e, t) {
	return {
		lo: e >>> 0,
		hi: t >>> 0
	};
}
function ua(e, t) {
	return {
		lo: e | 0,
		hi: t | 0
	};
}
function da(e, t) {
	return t = ~t, e ? e = ~e + 1 : t += 1, ua(e, t);
}
var fa = (e) => {
	let t = String(e);
	return "0000000".slice(t.length) + t;
};
function pa(e, t) {
	if (e >= 0) {
		for (; e > 127;) t.push(e & 127 | 128), e >>>= 7;
		t.push(e);
	} else {
		for (let n = 0; n < 9; n++) t.push(e & 127 | 128), e >>= 7;
		t.push(1);
	}
}
function ma() {
	let e = this.buf[this.pos++], t = e & 127;
	if (!(e & 128) || (e = this.buf[this.pos++], t |= (e & 127) << 7, !(e & 128)) || (e = this.buf[this.pos++], t |= (e & 127) << 14, !(e & 128)) || (e = this.buf[this.pos++], t |= (e & 127) << 21, !(e & 128))) return this.assertBounds(), t;
	e = this.buf[this.pos++], t |= (e & 15) << 28;
	for (let t = 5; e & 128 && t < 10; t++) e = this.buf[this.pos++];
	if (e & 128) throw Error("invalid varint");
	return this.assertBounds(), t >>> 0;
}
//#endregion
//#region node_modules/@bufbuild/protobuf/dist/esm/proto-int64.js
var G = /* @__PURE__ */ ha();
function ha() {
	let e = /* @__PURE__ */ new DataView(/* @__PURE__ */ new ArrayBuffer(8));
	if (typeof BigInt == "function" && typeof e.getBigInt64 == "function" && typeof e.getBigUint64 == "function" && typeof e.setBigInt64 == "function" && typeof e.setBigUint64 == "function" && (globalThis.Deno || typeof process != "object" || typeof process.env != "object" || process.env.BUF_BIGINT_DISABLE !== "1")) {
		let t = BigInt("-9223372036854775808"), n = BigInt("9223372036854775807"), r = BigInt("0"), i = BigInt("18446744073709551615");
		return {
			zero: BigInt(0),
			supported: !0,
			parse(e) {
				let r = typeof e == "bigint" ? e : BigInt(e);
				if (r > n || r < t) throw Error(`invalid int64: ${e}`);
				return r;
			},
			uParse(e) {
				let t = typeof e == "bigint" ? e : BigInt(e);
				if (t > i || t < r) throw Error(`invalid uint64: ${e}`);
				return t;
			},
			enc(t) {
				return e.setBigInt64(0, this.parse(t), !0), {
					lo: e.getInt32(0, !0),
					hi: e.getInt32(4, !0)
				};
			},
			uEnc(t) {
				return e.setBigInt64(0, this.uParse(t), !0), {
					lo: e.getInt32(0, !0),
					hi: e.getInt32(4, !0)
				};
			},
			dec(t, n) {
				return e.setInt32(0, t, !0), e.setInt32(4, n, !0), e.getBigInt64(0, !0);
			},
			uDec(t, n) {
				return e.setInt32(0, t, !0), e.setInt32(4, n, !0), e.getBigUint64(0, !0);
			}
		};
	}
	return {
		zero: "0",
		supported: !1,
		parse(e) {
			return typeof e != "string" && (e = e.toString()), ga(e), e;
		},
		uParse(e) {
			return typeof e != "string" && (e = e.toString()), _a(e), e;
		},
		enc(e) {
			return typeof e != "string" && (e = e.toString()), ga(e), oa(e);
		},
		uEnc(e) {
			return typeof e != "string" && (e = e.toString()), _a(e), oa(e);
		},
		dec(e, t) {
			return sa(e, t);
		},
		uDec(e, t) {
			return ca(e, t);
		}
	};
}
function ga(e) {
	if (!/^-?[0-9]+$/.test(e)) throw Error("invalid int64: " + e);
}
function _a(e) {
	if (!/^[0-9]+$/.test(e)) throw Error("invalid uint64: " + e);
}
//#endregion
//#region node_modules/@bufbuild/protobuf/dist/esm/wire/text-encoding.js
var va = Symbol.for("@bufbuild/protobuf/text-encoding");
function ya() {
	if (globalThis[va] == null) {
		let e = new globalThis.TextEncoder(), t = new globalThis.TextDecoder(), n;
		globalThis[va] = {
			encodeUtf8(t) {
				return e.encode(t);
			},
			decodeUtf8(e, r) {
				return r ? (n === void 0 && (n = new globalThis.TextDecoder("utf-8", { fatal: !0 })), n.decode(e)) : t.decode(e);
			},
			checkUtf8(e) {
				try {
					return !0;
				} catch {
					return !1;
				}
			}
		};
	}
	return globalThis[va];
}
//#endregion
//#region node_modules/@bufbuild/protobuf/dist/esm/wire/binary-encoding.js
var ba;
(function(e) {
	e[e.Varint = 0] = "Varint", e[e.Bit64 = 1] = "Bit64", e[e.LengthDelimited = 2] = "LengthDelimited", e[e.StartGroup = 3] = "StartGroup", e[e.EndGroup = 4] = "EndGroup", e[e.Bit32 = 5] = "Bit32";
})(ba ||= {});
var K = class {
	constructor(e = ya().encodeUtf8) {
		this.encodeUtf8 = e, this.stack = [], this.chunks = [], this.buf = [];
	}
	finish() {
		this.buf.length && (this.chunks.push(new Uint8Array(this.buf)), this.buf = []);
		let e = 0;
		for (let t = 0; t < this.chunks.length; t++) e += this.chunks[t].length;
		let t = new Uint8Array(e), n = 0;
		for (let e = 0; e < this.chunks.length; e++) t.set(this.chunks[e], n), n += this.chunks[e].length;
		return this.chunks = [], t;
	}
	fork() {
		return this.stack.push({
			chunks: this.chunks,
			buf: this.buf
		}), this.chunks = [], this.buf = [], this;
	}
	join() {
		let e = this.finish(), t = this.stack.pop();
		if (!t) throw Error("invalid state, fork stack empty");
		return this.chunks = t.chunks, this.buf = t.buf, this.uint32(e.byteLength), this.raw(e);
	}
	tag(e, t) {
		return this.uint32((e << 3 | t) >>> 0);
	}
	raw(e) {
		return this.buf.length && (this.chunks.push(new Uint8Array(this.buf)), this.buf = []), this.chunks.push(e), this;
	}
	uint32(e) {
		for (Sa(e); e > 127;) this.buf.push(e & 127 | 128), e >>>= 7;
		return this.buf.push(e), this;
	}
	int32(e) {
		return xa(e), pa(e, this.buf), this;
	}
	bool(e) {
		return this.buf.push(+!!e), this;
	}
	bytes(e) {
		return this.uint32(e.byteLength), this.raw(e);
	}
	string(e) {
		let t = this.encodeUtf8(e);
		return this.uint32(t.byteLength), this.raw(t);
	}
	float(e) {
		Ca(e);
		let t = new Uint8Array(4);
		return new DataView(t.buffer).setFloat32(0, e, !0), this.raw(t);
	}
	double(e) {
		let t = new Uint8Array(8);
		return new DataView(t.buffer).setFloat64(0, e, !0), this.raw(t);
	}
	fixed32(e) {
		Sa(e);
		let t = new Uint8Array(4);
		return new DataView(t.buffer).setUint32(0, e, !0), this.raw(t);
	}
	sfixed32(e) {
		xa(e);
		let t = new Uint8Array(4);
		return new DataView(t.buffer).setInt32(0, e, !0), this.raw(t);
	}
	sint32(e) {
		return xa(e), e = (e << 1 ^ e >> 31) >>> 0, pa(e, this.buf), this;
	}
	sfixed64(e) {
		let t = new Uint8Array(8), n = new DataView(t.buffer), r = G.enc(e);
		return n.setInt32(0, r.lo, !0), n.setInt32(4, r.hi, !0), this.raw(t);
	}
	fixed64(e) {
		let t = new Uint8Array(8), n = new DataView(t.buffer), r = G.uEnc(e);
		return n.setInt32(0, r.lo, !0), n.setInt32(4, r.hi, !0), this.raw(t);
	}
	int64(e) {
		let t = G.enc(e);
		return ia(t.lo, t.hi, this.buf), this;
	}
	sint64(e) {
		let t = G.enc(e), n = t.hi >> 31;
		return ia(t.lo << 1 ^ n, (t.hi << 1 | t.lo >>> 31) ^ n, this.buf), this;
	}
	uint64(e) {
		let t = G.uEnc(e);
		return ia(t.lo, t.hi, this.buf), this;
	}
}, q = class {
	constructor(e, t = ya().decodeUtf8) {
		this.decodeUtf8 = t, this.varint64 = ra, this.uint32 = ma, this.buf = e, this.len = e.length, this.pos = 0, this.view = new DataView(e.buffer, e.byteOffset, e.byteLength);
	}
	tag() {
		let e = this.pos, t = this.uint32(), n = this.pos - e;
		if (n > 5 || n == 5 && this.buf[this.pos - 1] > 15) throw Error("illegal tag: varint overflows uint32");
		let r = t >>> 3, i = t & 7;
		if (r <= 0 || i > 5) throw Error("illegal tag: field no " + r + " wire type " + i);
		return [r, i];
	}
	skip(e, t) {
		let n = this.pos;
		switch (e) {
			case ba.Varint:
				for (; this.buf[this.pos++] & 128;);
				break;
			case ba.Bit64: this.pos += 4;
			case ba.Bit32:
				this.pos += 4;
				break;
			case ba.LengthDelimited:
				let n = this.uint32();
				this.pos += n;
				break;
			case ba.StartGroup:
				for (;;) {
					let [e, n] = this.tag();
					if (n === ba.EndGroup) {
						if (t !== void 0 && e !== t) throw Error("invalid end group tag");
						break;
					}
					this.skip(n, e);
				}
				break;
			default: throw Error("cant skip wire type " + e);
		}
		return this.assertBounds(), this.buf.subarray(n, this.pos);
	}
	assertBounds() {
		if (this.pos > this.len) throw RangeError("premature EOF");
	}
	int32() {
		return this.uint32() | 0;
	}
	sint32() {
		let e = this.uint32();
		return e >>> 1 ^ -(e & 1);
	}
	int64() {
		return G.dec(...this.varint64());
	}
	uint64() {
		return G.uDec(...this.varint64());
	}
	sint64() {
		let [e, t] = this.varint64(), n = -(e & 1);
		return e = (e >>> 1 | (t & 1) << 31) ^ n, t = t >>> 1 ^ n, G.dec(e, t);
	}
	bool() {
		let [e, t] = this.varint64();
		return e !== 0 || t !== 0;
	}
	fixed32() {
		return this.view.getUint32((this.pos += 4) - 4, !0);
	}
	sfixed32() {
		return this.view.getInt32((this.pos += 4) - 4, !0);
	}
	fixed64() {
		return G.uDec(this.sfixed32(), this.sfixed32());
	}
	sfixed64() {
		return G.dec(this.sfixed32(), this.sfixed32());
	}
	float() {
		return this.view.getFloat32((this.pos += 4) - 4, !0);
	}
	double() {
		return this.view.getFloat64((this.pos += 8) - 8, !0);
	}
	bytes() {
		let e = this.uint32(), t = this.pos;
		return this.pos += e, this.assertBounds(), this.buf.subarray(t, t + e);
	}
	string(e) {
		return this.decodeUtf8(this.bytes(), e);
	}
};
function xa(e) {
	if (typeof e == "string") e = Number(e);
	else if (typeof e != "number") throw Error("invalid int32: " + typeof e);
	if (!Number.isInteger(e) || e > 2147483647 || e < -2147483648) throw Error("invalid int32: " + e);
}
function Sa(e) {
	if (typeof e == "string") e = Number(e);
	else if (typeof e != "number") throw Error("invalid uint32: " + typeof e);
	if (!Number.isInteger(e) || e > 4294967295 || e < 0) throw Error("invalid uint32: " + e);
}
function Ca(e) {
	if (typeof e == "string") {
		let t = e;
		if (e = Number(e), Number.isNaN(e) && t !== "NaN") throw Error("invalid float32: " + t);
	} else if (typeof e != "number") throw Error("invalid float32: " + typeof e);
	if (Number.isFinite(e) && (e > 34028234663852886e22 || e < -34028234663852886e22)) throw Error("invalid float32: " + e);
}
//#endregion
//#region node_modules/@ag-ui/proto/dist/index.mjs
var wa = /* @__PURE__ */ function(e) {
	return e[e.NULL_VALUE = 0] = "NULL_VALUE", e[e.UNRECOGNIZED = -1] = "UNRECOGNIZED", e;
}({});
function Ta() {
	return { fields: {} };
}
var Ea = {
	encode(e, t = new K()) {
		return Object.entries(e.fields).forEach(([e, n]) => {
			n !== void 0 && Oa.encode({
				key: e,
				value: n
			}, t.uint32(10).fork()).join();
		}), t;
	},
	decode(e, t) {
		let n = e instanceof q ? e : new q(e), r = t === void 0 ? n.len : n.pos + t, i = Ta();
		for (; n.pos < r;) {
			let e = n.uint32();
			switch (e >>> 3) {
				case 1: {
					if (e !== 10) break;
					let t = Oa.decode(n, n.uint32());
					t.value !== void 0 && (i.fields[t.key] = t.value);
					continue;
				}
			}
			if ((e & 7) == 4 || e === 0) break;
			n.skip(e & 7);
		}
		return i;
	},
	create(e) {
		return Ea.fromPartial(e ?? {});
	},
	fromPartial(e) {
		let t = Ta();
		return t.fields = Object.entries(e.fields ?? {}).reduce((e, [t, n]) => (n !== void 0 && (e[t] = n), e), {}), t;
	},
	wrap(e) {
		let t = Ta();
		if (e !== void 0) for (let n of Object.keys(e)) t.fields[n] = e[n];
		return t;
	},
	unwrap(e) {
		let t = {};
		if (e.fields) for (let n of Object.keys(e.fields)) t[n] = e.fields[n];
		return t;
	}
};
function Da() {
	return {
		key: "",
		value: void 0
	};
}
var Oa = {
	encode(e, t = new K()) {
		return e.key !== "" && t.uint32(10).string(e.key), e.value !== void 0 && J.encode(J.wrap(e.value), t.uint32(18).fork()).join(), t;
	},
	decode(e, t) {
		let n = e instanceof q ? e : new q(e), r = t === void 0 ? n.len : n.pos + t, i = Da();
		for (; n.pos < r;) {
			let e = n.uint32();
			switch (e >>> 3) {
				case 1:
					if (e !== 10) break;
					i.key = n.string();
					continue;
				case 2:
					if (e !== 18) break;
					i.value = J.unwrap(J.decode(n, n.uint32()));
					continue;
			}
			if ((e & 7) == 4 || e === 0) break;
			n.skip(e & 7);
		}
		return i;
	},
	create(e) {
		return Oa.fromPartial(e ?? {});
	},
	fromPartial(e) {
		let t = Da();
		return t.key = e.key ?? "", t.value = e.value ?? void 0, t;
	}
};
function ka() {
	return {
		nullValue: void 0,
		numberValue: void 0,
		stringValue: void 0,
		boolValue: void 0,
		structValue: void 0,
		listValue: void 0
	};
}
var J = {
	encode(e, t = new K()) {
		return e.nullValue !== void 0 && t.uint32(8).int32(e.nullValue), e.numberValue !== void 0 && t.uint32(17).double(e.numberValue), e.stringValue !== void 0 && t.uint32(26).string(e.stringValue), e.boolValue !== void 0 && t.uint32(32).bool(e.boolValue), e.structValue !== void 0 && Ea.encode(Ea.wrap(e.structValue), t.uint32(42).fork()).join(), e.listValue !== void 0 && ja.encode(ja.wrap(e.listValue), t.uint32(50).fork()).join(), t;
	},
	decode(e, t) {
		let n = e instanceof q ? e : new q(e), r = t === void 0 ? n.len : n.pos + t, i = ka();
		for (; n.pos < r;) {
			let e = n.uint32();
			switch (e >>> 3) {
				case 1:
					if (e !== 8) break;
					i.nullValue = n.int32();
					continue;
				case 2:
					if (e !== 17) break;
					i.numberValue = n.double();
					continue;
				case 3:
					if (e !== 26) break;
					i.stringValue = n.string();
					continue;
				case 4:
					if (e !== 32) break;
					i.boolValue = n.bool();
					continue;
				case 5:
					if (e !== 42) break;
					i.structValue = Ea.unwrap(Ea.decode(n, n.uint32()));
					continue;
				case 6:
					if (e !== 50) break;
					i.listValue = ja.unwrap(ja.decode(n, n.uint32()));
					continue;
			}
			if ((e & 7) == 4 || e === 0) break;
			n.skip(e & 7);
		}
		return i;
	},
	create(e) {
		return J.fromPartial(e ?? {});
	},
	fromPartial(e) {
		let t = ka();
		return t.nullValue = e.nullValue ?? void 0, t.numberValue = e.numberValue ?? void 0, t.stringValue = e.stringValue ?? void 0, t.boolValue = e.boolValue ?? void 0, t.structValue = e.structValue ?? void 0, t.listValue = e.listValue ?? void 0, t;
	},
	wrap(e) {
		let t = ka();
		if (e === null) t.nullValue = wa.NULL_VALUE;
		else if (typeof e == "boolean") t.boolValue = e;
		else if (typeof e == "number") t.numberValue = e;
		else if (typeof e == "string") t.stringValue = e;
		else if (globalThis.Array.isArray(e)) t.listValue = e;
		else if (typeof e == "object") t.structValue = e;
		else if (e !== void 0) throw new globalThis.Error("Unsupported any value type: " + typeof e);
		return t;
	},
	unwrap(e) {
		if (e.stringValue !== void 0) return e.stringValue;
		if (e?.numberValue !== void 0) return e.numberValue;
		if (e?.boolValue !== void 0) return e.boolValue;
		if (e?.structValue !== void 0) return e.structValue;
		if (e?.listValue !== void 0) return e.listValue;
		if (e?.nullValue !== void 0) return null;
	}
};
function Aa() {
	return { values: [] };
}
var ja = {
	encode(e, t = new K()) {
		for (let n of e.values) J.encode(J.wrap(n), t.uint32(10).fork()).join();
		return t;
	},
	decode(e, t) {
		let n = e instanceof q ? e : new q(e), r = t === void 0 ? n.len : n.pos + t, i = Aa();
		for (; n.pos < r;) {
			let e = n.uint32();
			switch (e >>> 3) {
				case 1:
					if (e !== 10) break;
					i.values.push(J.unwrap(J.decode(n, n.uint32())));
					continue;
			}
			if ((e & 7) == 4 || e === 0) break;
			n.skip(e & 7);
		}
		return i;
	},
	create(e) {
		return ja.fromPartial(e ?? {});
	},
	fromPartial(e) {
		let t = Aa();
		return t.values = e.values?.map((e) => e) || [], t;
	},
	wrap(e) {
		let t = Aa();
		return t.values = e ?? [], t;
	},
	unwrap(e) {
		return e?.hasOwnProperty("values") && globalThis.Array.isArray(e.values) ? e.values : e;
	}
}, Ma = /* @__PURE__ */ function(e) {
	return e[e.ADD = 0] = "ADD", e[e.REMOVE = 1] = "REMOVE", e[e.REPLACE = 2] = "REPLACE", e[e.MOVE = 3] = "MOVE", e[e.COPY = 4] = "COPY", e[e.TEST = 5] = "TEST", e[e.UNRECOGNIZED = -1] = "UNRECOGNIZED", e;
}({});
function Na() {
	return {
		op: 0,
		path: "",
		from: void 0,
		value: void 0
	};
}
var Pa = {
	encode(e, t = new K()) {
		return e.op !== 0 && t.uint32(8).int32(e.op), e.path !== "" && t.uint32(18).string(e.path), e.from !== void 0 && t.uint32(26).string(e.from), e.value !== void 0 && J.encode(J.wrap(e.value), t.uint32(34).fork()).join(), t;
	},
	decode(e, t) {
		let n = e instanceof q ? e : new q(e), r = t === void 0 ? n.len : n.pos + t, i = Na();
		for (; n.pos < r;) {
			let e = n.uint32();
			switch (e >>> 3) {
				case 1:
					if (e !== 8) break;
					i.op = n.int32();
					continue;
				case 2:
					if (e !== 18) break;
					i.path = n.string();
					continue;
				case 3:
					if (e !== 26) break;
					i.from = n.string();
					continue;
				case 4:
					if (e !== 34) break;
					i.value = J.unwrap(J.decode(n, n.uint32()));
					continue;
			}
			if ((e & 7) == 4 || e === 0) break;
			n.skip(e & 7);
		}
		return i;
	},
	create(e) {
		return Pa.fromPartial(e ?? {});
	},
	fromPartial(e) {
		let t = Na();
		return t.op = e.op ?? 0, t.path = e.path ?? "", t.from = e.from ?? void 0, t.value = e.value ?? void 0, t;
	}
};
function Fa() {
	return {
		id: "",
		type: "",
		function: void 0
	};
}
var Ia = {
	encode(e, t = new K()) {
		return e.id !== "" && t.uint32(10).string(e.id), e.type !== "" && t.uint32(18).string(e.type), e.function !== void 0 && Ra.encode(e.function, t.uint32(26).fork()).join(), t;
	},
	decode(e, t) {
		let n = e instanceof q ? e : new q(e), r = t === void 0 ? n.len : n.pos + t, i = Fa();
		for (; n.pos < r;) {
			let e = n.uint32();
			switch (e >>> 3) {
				case 1:
					if (e !== 10) break;
					i.id = n.string();
					continue;
				case 2:
					if (e !== 18) break;
					i.type = n.string();
					continue;
				case 3:
					if (e !== 26) break;
					i.function = Ra.decode(n, n.uint32());
					continue;
			}
			if ((e & 7) == 4 || e === 0) break;
			n.skip(e & 7);
		}
		return i;
	},
	create(e) {
		return Ia.fromPartial(e ?? {});
	},
	fromPartial(e) {
		let t = Fa();
		return t.id = e.id ?? "", t.type = e.type ?? "", t.function = e.function !== void 0 && e.function !== null ? Ra.fromPartial(e.function) : void 0, t;
	}
};
function La() {
	return {
		name: "",
		arguments: ""
	};
}
var Ra = {
	encode(e, t = new K()) {
		return e.name !== "" && t.uint32(10).string(e.name), e.arguments !== "" && t.uint32(18).string(e.arguments), t;
	},
	decode(e, t) {
		let n = e instanceof q ? e : new q(e), r = t === void 0 ? n.len : n.pos + t, i = La();
		for (; n.pos < r;) {
			let e = n.uint32();
			switch (e >>> 3) {
				case 1:
					if (e !== 10) break;
					i.name = n.string();
					continue;
				case 2:
					if (e !== 18) break;
					i.arguments = n.string();
					continue;
			}
			if ((e & 7) == 4 || e === 0) break;
			n.skip(e & 7);
		}
		return i;
	},
	create(e) {
		return Ra.fromPartial(e ?? {});
	},
	fromPartial(e) {
		let t = La();
		return t.name = e.name ?? "", t.arguments = e.arguments ?? "", t;
	}
};
function za() {
	return {
		value: "",
		mimeType: ""
	};
}
var Ba = {
	encode(e, t = new K()) {
		return e.value !== "" && t.uint32(10).string(e.value), e.mimeType !== "" && t.uint32(18).string(e.mimeType), t;
	},
	decode(e, t) {
		let n = e instanceof q ? e : new q(e), r = t === void 0 ? n.len : n.pos + t, i = za();
		for (; n.pos < r;) {
			let e = n.uint32();
			switch (e >>> 3) {
				case 1:
					if (e !== 10) break;
					i.value = n.string();
					continue;
				case 2:
					if (e !== 18) break;
					i.mimeType = n.string();
					continue;
			}
			if ((e & 7) == 4 || e === 0) break;
			n.skip(e & 7);
		}
		return i;
	},
	create(e) {
		return Ba.fromPartial(e ?? {});
	},
	fromPartial(e) {
		let t = za();
		return t.value = e.value ?? "", t.mimeType = e.mimeType ?? "", t;
	}
};
function Va() {
	return {
		value: "",
		mimeType: void 0
	};
}
var Ha = {
	encode(e, t = new K()) {
		return e.value !== "" && t.uint32(10).string(e.value), e.mimeType !== void 0 && t.uint32(18).string(e.mimeType), t;
	},
	decode(e, t) {
		let n = e instanceof q ? e : new q(e), r = t === void 0 ? n.len : n.pos + t, i = Va();
		for (; n.pos < r;) {
			let e = n.uint32();
			switch (e >>> 3) {
				case 1:
					if (e !== 10) break;
					i.value = n.string();
					continue;
				case 2:
					if (e !== 18) break;
					i.mimeType = n.string();
					continue;
			}
			if ((e & 7) == 4 || e === 0) break;
			n.skip(e & 7);
		}
		return i;
	},
	create(e) {
		return Ha.fromPartial(e ?? {});
	},
	fromPartial(e) {
		let t = Va();
		return t.value = e.value ?? "", t.mimeType = e.mimeType ?? void 0, t;
	}
};
function Ua() {
	return {
		data: void 0,
		url: void 0
	};
}
var Y = {
	encode(e, t = new K()) {
		return e.data !== void 0 && Ba.encode(e.data, t.uint32(10).fork()).join(), e.url !== void 0 && Ha.encode(e.url, t.uint32(18).fork()).join(), t;
	},
	decode(e, t) {
		let n = e instanceof q ? e : new q(e), r = t === void 0 ? n.len : n.pos + t, i = Ua();
		for (; n.pos < r;) {
			let e = n.uint32();
			switch (e >>> 3) {
				case 1:
					if (e !== 10) break;
					i.data = Ba.decode(n, n.uint32());
					continue;
				case 2:
					if (e !== 18) break;
					i.url = Ha.decode(n, n.uint32());
					continue;
			}
			if ((e & 7) == 4 || e === 0) break;
			n.skip(e & 7);
		}
		return i;
	},
	create(e) {
		return Y.fromPartial(e ?? {});
	},
	fromPartial(e) {
		let t = Ua();
		return t.data = e.data !== void 0 && e.data !== null ? Ba.fromPartial(e.data) : void 0, t.url = e.url !== void 0 && e.url !== null ? Ha.fromPartial(e.url) : void 0, t;
	}
};
function Wa() {
	return { text: "" };
}
var Ga = {
	encode(e, t = new K()) {
		return e.text !== "" && t.uint32(10).string(e.text), t;
	},
	decode(e, t) {
		let n = e instanceof q ? e : new q(e), r = t === void 0 ? n.len : n.pos + t, i = Wa();
		for (; n.pos < r;) {
			let e = n.uint32();
			switch (e >>> 3) {
				case 1:
					if (e !== 10) break;
					i.text = n.string();
					continue;
			}
			if ((e & 7) == 4 || e === 0) break;
			n.skip(e & 7);
		}
		return i;
	},
	create(e) {
		return Ga.fromPartial(e ?? {});
	},
	fromPartial(e) {
		let t = Wa();
		return t.text = e.text ?? "", t;
	}
};
function Ka() {
	return {
		source: void 0,
		metadata: void 0
	};
}
var qa = {
	encode(e, t = new K()) {
		return e.source !== void 0 && Y.encode(e.source, t.uint32(10).fork()).join(), e.metadata !== void 0 && J.encode(J.wrap(e.metadata), t.uint32(18).fork()).join(), t;
	},
	decode(e, t) {
		let n = e instanceof q ? e : new q(e), r = t === void 0 ? n.len : n.pos + t, i = Ka();
		for (; n.pos < r;) {
			let e = n.uint32();
			switch (e >>> 3) {
				case 1:
					if (e !== 10) break;
					i.source = Y.decode(n, n.uint32());
					continue;
				case 2:
					if (e !== 18) break;
					i.metadata = J.unwrap(J.decode(n, n.uint32()));
					continue;
			}
			if ((e & 7) == 4 || e === 0) break;
			n.skip(e & 7);
		}
		return i;
	},
	create(e) {
		return qa.fromPartial(e ?? {});
	},
	fromPartial(e) {
		let t = Ka();
		return t.source = e.source !== void 0 && e.source !== null ? Y.fromPartial(e.source) : void 0, t.metadata = e.metadata ?? void 0, t;
	}
};
function Ja() {
	return {
		source: void 0,
		metadata: void 0
	};
}
var Ya = {
	encode(e, t = new K()) {
		return e.source !== void 0 && Y.encode(e.source, t.uint32(10).fork()).join(), e.metadata !== void 0 && J.encode(J.wrap(e.metadata), t.uint32(18).fork()).join(), t;
	},
	decode(e, t) {
		let n = e instanceof q ? e : new q(e), r = t === void 0 ? n.len : n.pos + t, i = Ja();
		for (; n.pos < r;) {
			let e = n.uint32();
			switch (e >>> 3) {
				case 1:
					if (e !== 10) break;
					i.source = Y.decode(n, n.uint32());
					continue;
				case 2:
					if (e !== 18) break;
					i.metadata = J.unwrap(J.decode(n, n.uint32()));
					continue;
			}
			if ((e & 7) == 4 || e === 0) break;
			n.skip(e & 7);
		}
		return i;
	},
	create(e) {
		return Ya.fromPartial(e ?? {});
	},
	fromPartial(e) {
		let t = Ja();
		return t.source = e.source !== void 0 && e.source !== null ? Y.fromPartial(e.source) : void 0, t.metadata = e.metadata ?? void 0, t;
	}
};
function Xa() {
	return {
		source: void 0,
		metadata: void 0
	};
}
var Za = {
	encode(e, t = new K()) {
		return e.source !== void 0 && Y.encode(e.source, t.uint32(10).fork()).join(), e.metadata !== void 0 && J.encode(J.wrap(e.metadata), t.uint32(18).fork()).join(), t;
	},
	decode(e, t) {
		let n = e instanceof q ? e : new q(e), r = t === void 0 ? n.len : n.pos + t, i = Xa();
		for (; n.pos < r;) {
			let e = n.uint32();
			switch (e >>> 3) {
				case 1:
					if (e !== 10) break;
					i.source = Y.decode(n, n.uint32());
					continue;
				case 2:
					if (e !== 18) break;
					i.metadata = J.unwrap(J.decode(n, n.uint32()));
					continue;
			}
			if ((e & 7) == 4 || e === 0) break;
			n.skip(e & 7);
		}
		return i;
	},
	create(e) {
		return Za.fromPartial(e ?? {});
	},
	fromPartial(e) {
		let t = Xa();
		return t.source = e.source !== void 0 && e.source !== null ? Y.fromPartial(e.source) : void 0, t.metadata = e.metadata ?? void 0, t;
	}
};
function Qa() {
	return {
		source: void 0,
		metadata: void 0
	};
}
var $a = {
	encode(e, t = new K()) {
		return e.source !== void 0 && Y.encode(e.source, t.uint32(10).fork()).join(), e.metadata !== void 0 && J.encode(J.wrap(e.metadata), t.uint32(18).fork()).join(), t;
	},
	decode(e, t) {
		let n = e instanceof q ? e : new q(e), r = t === void 0 ? n.len : n.pos + t, i = Qa();
		for (; n.pos < r;) {
			let e = n.uint32();
			switch (e >>> 3) {
				case 1:
					if (e !== 10) break;
					i.source = Y.decode(n, n.uint32());
					continue;
				case 2:
					if (e !== 18) break;
					i.metadata = J.unwrap(J.decode(n, n.uint32()));
					continue;
			}
			if ((e & 7) == 4 || e === 0) break;
			n.skip(e & 7);
		}
		return i;
	},
	create(e) {
		return $a.fromPartial(e ?? {});
	},
	fromPartial(e) {
		let t = Qa();
		return t.source = e.source !== void 0 && e.source !== null ? Y.fromPartial(e.source) : void 0, t.metadata = e.metadata ?? void 0, t;
	}
};
function eo() {
	return {
		text: void 0,
		image: void 0,
		audio: void 0,
		video: void 0,
		document: void 0
	};
}
var to = {
	encode(e, t = new K()) {
		return e.text !== void 0 && Ga.encode(e.text, t.uint32(10).fork()).join(), e.image !== void 0 && qa.encode(e.image, t.uint32(18).fork()).join(), e.audio !== void 0 && Ya.encode(e.audio, t.uint32(26).fork()).join(), e.video !== void 0 && Za.encode(e.video, t.uint32(34).fork()).join(), e.document !== void 0 && $a.encode(e.document, t.uint32(42).fork()).join(), t;
	},
	decode(e, t) {
		let n = e instanceof q ? e : new q(e), r = t === void 0 ? n.len : n.pos + t, i = eo();
		for (; n.pos < r;) {
			let e = n.uint32();
			switch (e >>> 3) {
				case 1:
					if (e !== 10) break;
					i.text = Ga.decode(n, n.uint32());
					continue;
				case 2:
					if (e !== 18) break;
					i.image = qa.decode(n, n.uint32());
					continue;
				case 3:
					if (e !== 26) break;
					i.audio = Ya.decode(n, n.uint32());
					continue;
				case 4:
					if (e !== 34) break;
					i.video = Za.decode(n, n.uint32());
					continue;
				case 5:
					if (e !== 42) break;
					i.document = $a.decode(n, n.uint32());
					continue;
			}
			if ((e & 7) == 4 || e === 0) break;
			n.skip(e & 7);
		}
		return i;
	},
	create(e) {
		return to.fromPartial(e ?? {});
	},
	fromPartial(e) {
		let t = eo();
		return t.text = e.text !== void 0 && e.text !== null ? Ga.fromPartial(e.text) : void 0, t.image = e.image !== void 0 && e.image !== null ? qa.fromPartial(e.image) : void 0, t.audio = e.audio !== void 0 && e.audio !== null ? Ya.fromPartial(e.audio) : void 0, t.video = e.video !== void 0 && e.video !== null ? Za.fromPartial(e.video) : void 0, t.document = e.document !== void 0 && e.document !== null ? $a.fromPartial(e.document) : void 0, t;
	}
};
function no() {
	return {
		id: "",
		role: "",
		content: void 0,
		name: void 0,
		toolCalls: [],
		toolCallId: void 0,
		error: void 0,
		contentParts: []
	};
}
var ro = {
	encode(e, t = new K()) {
		e.id !== "" && t.uint32(10).string(e.id), e.role !== "" && t.uint32(18).string(e.role), e.content !== void 0 && t.uint32(26).string(e.content), e.name !== void 0 && t.uint32(34).string(e.name);
		for (let n of e.toolCalls) Ia.encode(n, t.uint32(42).fork()).join();
		e.toolCallId !== void 0 && t.uint32(50).string(e.toolCallId), e.error !== void 0 && t.uint32(58).string(e.error);
		for (let n of e.contentParts) to.encode(n, t.uint32(66).fork()).join();
		return t;
	},
	decode(e, t) {
		let n = e instanceof q ? e : new q(e), r = t === void 0 ? n.len : n.pos + t, i = no();
		for (; n.pos < r;) {
			let e = n.uint32();
			switch (e >>> 3) {
				case 1:
					if (e !== 10) break;
					i.id = n.string();
					continue;
				case 2:
					if (e !== 18) break;
					i.role = n.string();
					continue;
				case 3:
					if (e !== 26) break;
					i.content = n.string();
					continue;
				case 4:
					if (e !== 34) break;
					i.name = n.string();
					continue;
				case 5:
					if (e !== 42) break;
					i.toolCalls.push(Ia.decode(n, n.uint32()));
					continue;
				case 6:
					if (e !== 50) break;
					i.toolCallId = n.string();
					continue;
				case 7:
					if (e !== 58) break;
					i.error = n.string();
					continue;
				case 8:
					if (e !== 66) break;
					i.contentParts.push(to.decode(n, n.uint32()));
					continue;
			}
			if ((e & 7) == 4 || e === 0) break;
			n.skip(e & 7);
		}
		return i;
	},
	create(e) {
		return ro.fromPartial(e ?? {});
	},
	fromPartial(e) {
		let t = no();
		return t.id = e.id ?? "", t.role = e.role ?? "", t.content = e.content ?? void 0, t.name = e.name ?? void 0, t.toolCalls = e.toolCalls?.map((e) => Ia.fromPartial(e)) || [], t.toolCallId = e.toolCallId ?? void 0, t.error = e.error ?? void 0, t.contentParts = e.contentParts?.map((e) => to.fromPartial(e)) || [], t;
	}
}, io = /* @__PURE__ */ function(e) {
	return e[e.TEXT_MESSAGE_START = 0] = "TEXT_MESSAGE_START", e[e.TEXT_MESSAGE_CONTENT = 1] = "TEXT_MESSAGE_CONTENT", e[e.TEXT_MESSAGE_END = 2] = "TEXT_MESSAGE_END", e[e.TOOL_CALL_START = 3] = "TOOL_CALL_START", e[e.TOOL_CALL_ARGS = 4] = "TOOL_CALL_ARGS", e[e.TOOL_CALL_END = 5] = "TOOL_CALL_END", e[e.STATE_SNAPSHOT = 6] = "STATE_SNAPSHOT", e[e.STATE_DELTA = 7] = "STATE_DELTA", e[e.MESSAGES_SNAPSHOT = 8] = "MESSAGES_SNAPSHOT", e[e.RAW = 9] = "RAW", e[e.CUSTOM = 10] = "CUSTOM", e[e.RUN_STARTED = 11] = "RUN_STARTED", e[e.RUN_FINISHED = 12] = "RUN_FINISHED", e[e.RUN_ERROR = 13] = "RUN_ERROR", e[e.STEP_STARTED = 14] = "STEP_STARTED", e[e.STEP_FINISHED = 15] = "STEP_FINISHED", e[e.UNRECOGNIZED = -1] = "UNRECOGNIZED", e;
}({});
function ao() {
	return {
		type: 0,
		timestamp: void 0,
		rawEvent: void 0
	};
}
var X = {
	encode(e, t = new K()) {
		return e.type !== 0 && t.uint32(8).int32(e.type), e.timestamp !== void 0 && t.uint32(16).int64(e.timestamp), e.rawEvent !== void 0 && J.encode(J.wrap(e.rawEvent), t.uint32(26).fork()).join(), t;
	},
	decode(e, t) {
		let n = e instanceof q ? e : new q(e), r = t === void 0 ? n.len : n.pos + t, i = ao();
		for (; n.pos < r;) {
			let e = n.uint32();
			switch (e >>> 3) {
				case 1:
					if (e !== 8) break;
					i.type = n.int32();
					continue;
				case 2:
					if (e !== 16) break;
					i.timestamp = Go(n.int64());
					continue;
				case 3:
					if (e !== 26) break;
					i.rawEvent = J.unwrap(J.decode(n, n.uint32()));
					continue;
			}
			if ((e & 7) == 4 || e === 0) break;
			n.skip(e & 7);
		}
		return i;
	},
	create(e) {
		return X.fromPartial(e ?? {});
	},
	fromPartial(e) {
		let t = ao();
		return t.type = e.type ?? 0, t.timestamp = e.timestamp ?? void 0, t.rawEvent = e.rawEvent ?? void 0, t;
	}
};
function oo() {
	return {
		baseEvent: void 0,
		messageId: "",
		role: void 0,
		name: void 0
	};
}
var so = {
	encode(e, t = new K()) {
		return e.baseEvent !== void 0 && X.encode(e.baseEvent, t.uint32(10).fork()).join(), e.messageId !== "" && t.uint32(18).string(e.messageId), e.role !== void 0 && t.uint32(26).string(e.role), e.name !== void 0 && t.uint32(34).string(e.name), t;
	},
	decode(e, t) {
		let n = e instanceof q ? e : new q(e), r = t === void 0 ? n.len : n.pos + t, i = oo();
		for (; n.pos < r;) {
			let e = n.uint32();
			switch (e >>> 3) {
				case 1:
					if (e !== 10) break;
					i.baseEvent = X.decode(n, n.uint32());
					continue;
				case 2:
					if (e !== 18) break;
					i.messageId = n.string();
					continue;
				case 3:
					if (e !== 26) break;
					i.role = n.string();
					continue;
				case 4:
					if (e !== 34) break;
					i.name = n.string();
					continue;
			}
			if ((e & 7) == 4 || e === 0) break;
			n.skip(e & 7);
		}
		return i;
	},
	create(e) {
		return so.fromPartial(e ?? {});
	},
	fromPartial(e) {
		let t = oo();
		return t.baseEvent = e.baseEvent !== void 0 && e.baseEvent !== null ? X.fromPartial(e.baseEvent) : void 0, t.messageId = e.messageId ?? "", t.role = e.role ?? void 0, t.name = e.name ?? void 0, t;
	}
};
function co() {
	return {
		baseEvent: void 0,
		messageId: "",
		delta: ""
	};
}
var lo = {
	encode(e, t = new K()) {
		return e.baseEvent !== void 0 && X.encode(e.baseEvent, t.uint32(10).fork()).join(), e.messageId !== "" && t.uint32(18).string(e.messageId), e.delta !== "" && t.uint32(26).string(e.delta), t;
	},
	decode(e, t) {
		let n = e instanceof q ? e : new q(e), r = t === void 0 ? n.len : n.pos + t, i = co();
		for (; n.pos < r;) {
			let e = n.uint32();
			switch (e >>> 3) {
				case 1:
					if (e !== 10) break;
					i.baseEvent = X.decode(n, n.uint32());
					continue;
				case 2:
					if (e !== 18) break;
					i.messageId = n.string();
					continue;
				case 3:
					if (e !== 26) break;
					i.delta = n.string();
					continue;
			}
			if ((e & 7) == 4 || e === 0) break;
			n.skip(e & 7);
		}
		return i;
	},
	create(e) {
		return lo.fromPartial(e ?? {});
	},
	fromPartial(e) {
		let t = co();
		return t.baseEvent = e.baseEvent !== void 0 && e.baseEvent !== null ? X.fromPartial(e.baseEvent) : void 0, t.messageId = e.messageId ?? "", t.delta = e.delta ?? "", t;
	}
};
function uo() {
	return {
		baseEvent: void 0,
		messageId: ""
	};
}
var fo = {
	encode(e, t = new K()) {
		return e.baseEvent !== void 0 && X.encode(e.baseEvent, t.uint32(10).fork()).join(), e.messageId !== "" && t.uint32(18).string(e.messageId), t;
	},
	decode(e, t) {
		let n = e instanceof q ? e : new q(e), r = t === void 0 ? n.len : n.pos + t, i = uo();
		for (; n.pos < r;) {
			let e = n.uint32();
			switch (e >>> 3) {
				case 1:
					if (e !== 10) break;
					i.baseEvent = X.decode(n, n.uint32());
					continue;
				case 2:
					if (e !== 18) break;
					i.messageId = n.string();
					continue;
			}
			if ((e & 7) == 4 || e === 0) break;
			n.skip(e & 7);
		}
		return i;
	},
	create(e) {
		return fo.fromPartial(e ?? {});
	},
	fromPartial(e) {
		let t = uo();
		return t.baseEvent = e.baseEvent !== void 0 && e.baseEvent !== null ? X.fromPartial(e.baseEvent) : void 0, t.messageId = e.messageId ?? "", t;
	}
};
function po() {
	return {
		baseEvent: void 0,
		toolCallId: "",
		toolCallName: "",
		parentMessageId: void 0
	};
}
var mo = {
	encode(e, t = new K()) {
		return e.baseEvent !== void 0 && X.encode(e.baseEvent, t.uint32(10).fork()).join(), e.toolCallId !== "" && t.uint32(18).string(e.toolCallId), e.toolCallName !== "" && t.uint32(26).string(e.toolCallName), e.parentMessageId !== void 0 && t.uint32(34).string(e.parentMessageId), t;
	},
	decode(e, t) {
		let n = e instanceof q ? e : new q(e), r = t === void 0 ? n.len : n.pos + t, i = po();
		for (; n.pos < r;) {
			let e = n.uint32();
			switch (e >>> 3) {
				case 1:
					if (e !== 10) break;
					i.baseEvent = X.decode(n, n.uint32());
					continue;
				case 2:
					if (e !== 18) break;
					i.toolCallId = n.string();
					continue;
				case 3:
					if (e !== 26) break;
					i.toolCallName = n.string();
					continue;
				case 4:
					if (e !== 34) break;
					i.parentMessageId = n.string();
					continue;
			}
			if ((e & 7) == 4 || e === 0) break;
			n.skip(e & 7);
		}
		return i;
	},
	create(e) {
		return mo.fromPartial(e ?? {});
	},
	fromPartial(e) {
		let t = po();
		return t.baseEvent = e.baseEvent !== void 0 && e.baseEvent !== null ? X.fromPartial(e.baseEvent) : void 0, t.toolCallId = e.toolCallId ?? "", t.toolCallName = e.toolCallName ?? "", t.parentMessageId = e.parentMessageId ?? void 0, t;
	}
};
function ho() {
	return {
		baseEvent: void 0,
		toolCallId: "",
		delta: ""
	};
}
var go = {
	encode(e, t = new K()) {
		return e.baseEvent !== void 0 && X.encode(e.baseEvent, t.uint32(10).fork()).join(), e.toolCallId !== "" && t.uint32(18).string(e.toolCallId), e.delta !== "" && t.uint32(26).string(e.delta), t;
	},
	decode(e, t) {
		let n = e instanceof q ? e : new q(e), r = t === void 0 ? n.len : n.pos + t, i = ho();
		for (; n.pos < r;) {
			let e = n.uint32();
			switch (e >>> 3) {
				case 1:
					if (e !== 10) break;
					i.baseEvent = X.decode(n, n.uint32());
					continue;
				case 2:
					if (e !== 18) break;
					i.toolCallId = n.string();
					continue;
				case 3:
					if (e !== 26) break;
					i.delta = n.string();
					continue;
			}
			if ((e & 7) == 4 || e === 0) break;
			n.skip(e & 7);
		}
		return i;
	},
	create(e) {
		return go.fromPartial(e ?? {});
	},
	fromPartial(e) {
		let t = ho();
		return t.baseEvent = e.baseEvent !== void 0 && e.baseEvent !== null ? X.fromPartial(e.baseEvent) : void 0, t.toolCallId = e.toolCallId ?? "", t.delta = e.delta ?? "", t;
	}
};
function _o() {
	return {
		baseEvent: void 0,
		toolCallId: ""
	};
}
var vo = {
	encode(e, t = new K()) {
		return e.baseEvent !== void 0 && X.encode(e.baseEvent, t.uint32(10).fork()).join(), e.toolCallId !== "" && t.uint32(18).string(e.toolCallId), t;
	},
	decode(e, t) {
		let n = e instanceof q ? e : new q(e), r = t === void 0 ? n.len : n.pos + t, i = _o();
		for (; n.pos < r;) {
			let e = n.uint32();
			switch (e >>> 3) {
				case 1:
					if (e !== 10) break;
					i.baseEvent = X.decode(n, n.uint32());
					continue;
				case 2:
					if (e !== 18) break;
					i.toolCallId = n.string();
					continue;
			}
			if ((e & 7) == 4 || e === 0) break;
			n.skip(e & 7);
		}
		return i;
	},
	create(e) {
		return vo.fromPartial(e ?? {});
	},
	fromPartial(e) {
		let t = _o();
		return t.baseEvent = e.baseEvent !== void 0 && e.baseEvent !== null ? X.fromPartial(e.baseEvent) : void 0, t.toolCallId = e.toolCallId ?? "", t;
	}
};
function yo() {
	return {
		baseEvent: void 0,
		snapshot: void 0
	};
}
var bo = {
	encode(e, t = new K()) {
		return e.baseEvent !== void 0 && X.encode(e.baseEvent, t.uint32(10).fork()).join(), e.snapshot !== void 0 && J.encode(J.wrap(e.snapshot), t.uint32(18).fork()).join(), t;
	},
	decode(e, t) {
		let n = e instanceof q ? e : new q(e), r = t === void 0 ? n.len : n.pos + t, i = yo();
		for (; n.pos < r;) {
			let e = n.uint32();
			switch (e >>> 3) {
				case 1:
					if (e !== 10) break;
					i.baseEvent = X.decode(n, n.uint32());
					continue;
				case 2:
					if (e !== 18) break;
					i.snapshot = J.unwrap(J.decode(n, n.uint32()));
					continue;
			}
			if ((e & 7) == 4 || e === 0) break;
			n.skip(e & 7);
		}
		return i;
	},
	create(e) {
		return bo.fromPartial(e ?? {});
	},
	fromPartial(e) {
		let t = yo();
		return t.baseEvent = e.baseEvent !== void 0 && e.baseEvent !== null ? X.fromPartial(e.baseEvent) : void 0, t.snapshot = e.snapshot ?? void 0, t;
	}
};
function xo() {
	return {
		baseEvent: void 0,
		delta: []
	};
}
var So = {
	encode(e, t = new K()) {
		e.baseEvent !== void 0 && X.encode(e.baseEvent, t.uint32(10).fork()).join();
		for (let n of e.delta) Pa.encode(n, t.uint32(18).fork()).join();
		return t;
	},
	decode(e, t) {
		let n = e instanceof q ? e : new q(e), r = t === void 0 ? n.len : n.pos + t, i = xo();
		for (; n.pos < r;) {
			let e = n.uint32();
			switch (e >>> 3) {
				case 1:
					if (e !== 10) break;
					i.baseEvent = X.decode(n, n.uint32());
					continue;
				case 2:
					if (e !== 18) break;
					i.delta.push(Pa.decode(n, n.uint32()));
					continue;
			}
			if ((e & 7) == 4 || e === 0) break;
			n.skip(e & 7);
		}
		return i;
	},
	create(e) {
		return So.fromPartial(e ?? {});
	},
	fromPartial(e) {
		let t = xo();
		return t.baseEvent = e.baseEvent !== void 0 && e.baseEvent !== null ? X.fromPartial(e.baseEvent) : void 0, t.delta = e.delta?.map((e) => Pa.fromPartial(e)) || [], t;
	}
};
function Co() {
	return {
		baseEvent: void 0,
		messages: []
	};
}
var wo = {
	encode(e, t = new K()) {
		e.baseEvent !== void 0 && X.encode(e.baseEvent, t.uint32(10).fork()).join();
		for (let n of e.messages) ro.encode(n, t.uint32(18).fork()).join();
		return t;
	},
	decode(e, t) {
		let n = e instanceof q ? e : new q(e), r = t === void 0 ? n.len : n.pos + t, i = Co();
		for (; n.pos < r;) {
			let e = n.uint32();
			switch (e >>> 3) {
				case 1:
					if (e !== 10) break;
					i.baseEvent = X.decode(n, n.uint32());
					continue;
				case 2:
					if (e !== 18) break;
					i.messages.push(ro.decode(n, n.uint32()));
					continue;
			}
			if ((e & 7) == 4 || e === 0) break;
			n.skip(e & 7);
		}
		return i;
	},
	create(e) {
		return wo.fromPartial(e ?? {});
	},
	fromPartial(e) {
		let t = Co();
		return t.baseEvent = e.baseEvent !== void 0 && e.baseEvent !== null ? X.fromPartial(e.baseEvent) : void 0, t.messages = e.messages?.map((e) => ro.fromPartial(e)) || [], t;
	}
};
function To() {
	return {
		baseEvent: void 0,
		event: void 0,
		source: void 0
	};
}
var Eo = {
	encode(e, t = new K()) {
		return e.baseEvent !== void 0 && X.encode(e.baseEvent, t.uint32(10).fork()).join(), e.event !== void 0 && J.encode(J.wrap(e.event), t.uint32(18).fork()).join(), e.source !== void 0 && t.uint32(26).string(e.source), t;
	},
	decode(e, t) {
		let n = e instanceof q ? e : new q(e), r = t === void 0 ? n.len : n.pos + t, i = To();
		for (; n.pos < r;) {
			let e = n.uint32();
			switch (e >>> 3) {
				case 1:
					if (e !== 10) break;
					i.baseEvent = X.decode(n, n.uint32());
					continue;
				case 2:
					if (e !== 18) break;
					i.event = J.unwrap(J.decode(n, n.uint32()));
					continue;
				case 3:
					if (e !== 26) break;
					i.source = n.string();
					continue;
			}
			if ((e & 7) == 4 || e === 0) break;
			n.skip(e & 7);
		}
		return i;
	},
	create(e) {
		return Eo.fromPartial(e ?? {});
	},
	fromPartial(e) {
		let t = To();
		return t.baseEvent = e.baseEvent !== void 0 && e.baseEvent !== null ? X.fromPartial(e.baseEvent) : void 0, t.event = e.event ?? void 0, t.source = e.source ?? void 0, t;
	}
};
function Do() {
	return {
		baseEvent: void 0,
		name: "",
		value: void 0
	};
}
var Oo = {
	encode(e, t = new K()) {
		return e.baseEvent !== void 0 && X.encode(e.baseEvent, t.uint32(10).fork()).join(), e.name !== "" && t.uint32(18).string(e.name), e.value !== void 0 && J.encode(J.wrap(e.value), t.uint32(26).fork()).join(), t;
	},
	decode(e, t) {
		let n = e instanceof q ? e : new q(e), r = t === void 0 ? n.len : n.pos + t, i = Do();
		for (; n.pos < r;) {
			let e = n.uint32();
			switch (e >>> 3) {
				case 1:
					if (e !== 10) break;
					i.baseEvent = X.decode(n, n.uint32());
					continue;
				case 2:
					if (e !== 18) break;
					i.name = n.string();
					continue;
				case 3:
					if (e !== 26) break;
					i.value = J.unwrap(J.decode(n, n.uint32()));
					continue;
			}
			if ((e & 7) == 4 || e === 0) break;
			n.skip(e & 7);
		}
		return i;
	},
	create(e) {
		return Oo.fromPartial(e ?? {});
	},
	fromPartial(e) {
		let t = Do();
		return t.baseEvent = e.baseEvent !== void 0 && e.baseEvent !== null ? X.fromPartial(e.baseEvent) : void 0, t.name = e.name ?? "", t.value = e.value ?? void 0, t;
	}
};
function ko() {
	return {
		baseEvent: void 0,
		threadId: "",
		runId: ""
	};
}
var Ao = {
	encode(e, t = new K()) {
		return e.baseEvent !== void 0 && X.encode(e.baseEvent, t.uint32(10).fork()).join(), e.threadId !== "" && t.uint32(18).string(e.threadId), e.runId !== "" && t.uint32(26).string(e.runId), t;
	},
	decode(e, t) {
		let n = e instanceof q ? e : new q(e), r = t === void 0 ? n.len : n.pos + t, i = ko();
		for (; n.pos < r;) {
			let e = n.uint32();
			switch (e >>> 3) {
				case 1:
					if (e !== 10) break;
					i.baseEvent = X.decode(n, n.uint32());
					continue;
				case 2:
					if (e !== 18) break;
					i.threadId = n.string();
					continue;
				case 3:
					if (e !== 26) break;
					i.runId = n.string();
					continue;
			}
			if ((e & 7) == 4 || e === 0) break;
			n.skip(e & 7);
		}
		return i;
	},
	create(e) {
		return Ao.fromPartial(e ?? {});
	},
	fromPartial(e) {
		let t = ko();
		return t.baseEvent = e.baseEvent !== void 0 && e.baseEvent !== null ? X.fromPartial(e.baseEvent) : void 0, t.threadId = e.threadId ?? "", t.runId = e.runId ?? "", t;
	}
};
function jo() {
	return {
		baseEvent: void 0,
		threadId: "",
		runId: "",
		result: void 0
	};
}
var Mo = {
	encode(e, t = new K()) {
		return e.baseEvent !== void 0 && X.encode(e.baseEvent, t.uint32(10).fork()).join(), e.threadId !== "" && t.uint32(18).string(e.threadId), e.runId !== "" && t.uint32(26).string(e.runId), e.result !== void 0 && J.encode(J.wrap(e.result), t.uint32(34).fork()).join(), t;
	},
	decode(e, t) {
		let n = e instanceof q ? e : new q(e), r = t === void 0 ? n.len : n.pos + t, i = jo();
		for (; n.pos < r;) {
			let e = n.uint32();
			switch (e >>> 3) {
				case 1:
					if (e !== 10) break;
					i.baseEvent = X.decode(n, n.uint32());
					continue;
				case 2:
					if (e !== 18) break;
					i.threadId = n.string();
					continue;
				case 3:
					if (e !== 26) break;
					i.runId = n.string();
					continue;
				case 4:
					if (e !== 34) break;
					i.result = J.unwrap(J.decode(n, n.uint32()));
					continue;
			}
			if ((e & 7) == 4 || e === 0) break;
			n.skip(e & 7);
		}
		return i;
	},
	create(e) {
		return Mo.fromPartial(e ?? {});
	},
	fromPartial(e) {
		let t = jo();
		return t.baseEvent = e.baseEvent !== void 0 && e.baseEvent !== null ? X.fromPartial(e.baseEvent) : void 0, t.threadId = e.threadId ?? "", t.runId = e.runId ?? "", t.result = e.result ?? void 0, t;
	}
};
function No() {
	return {
		baseEvent: void 0,
		code: void 0,
		message: ""
	};
}
var Po = {
	encode(e, t = new K()) {
		return e.baseEvent !== void 0 && X.encode(e.baseEvent, t.uint32(10).fork()).join(), e.code !== void 0 && t.uint32(18).string(e.code), e.message !== "" && t.uint32(26).string(e.message), t;
	},
	decode(e, t) {
		let n = e instanceof q ? e : new q(e), r = t === void 0 ? n.len : n.pos + t, i = No();
		for (; n.pos < r;) {
			let e = n.uint32();
			switch (e >>> 3) {
				case 1:
					if (e !== 10) break;
					i.baseEvent = X.decode(n, n.uint32());
					continue;
				case 2:
					if (e !== 18) break;
					i.code = n.string();
					continue;
				case 3:
					if (e !== 26) break;
					i.message = n.string();
					continue;
			}
			if ((e & 7) == 4 || e === 0) break;
			n.skip(e & 7);
		}
		return i;
	},
	create(e) {
		return Po.fromPartial(e ?? {});
	},
	fromPartial(e) {
		let t = No();
		return t.baseEvent = e.baseEvent !== void 0 && e.baseEvent !== null ? X.fromPartial(e.baseEvent) : void 0, t.code = e.code ?? void 0, t.message = e.message ?? "", t;
	}
};
function Fo() {
	return {
		baseEvent: void 0,
		stepName: ""
	};
}
var Io = {
	encode(e, t = new K()) {
		return e.baseEvent !== void 0 && X.encode(e.baseEvent, t.uint32(10).fork()).join(), e.stepName !== "" && t.uint32(18).string(e.stepName), t;
	},
	decode(e, t) {
		let n = e instanceof q ? e : new q(e), r = t === void 0 ? n.len : n.pos + t, i = Fo();
		for (; n.pos < r;) {
			let e = n.uint32();
			switch (e >>> 3) {
				case 1:
					if (e !== 10) break;
					i.baseEvent = X.decode(n, n.uint32());
					continue;
				case 2:
					if (e !== 18) break;
					i.stepName = n.string();
					continue;
			}
			if ((e & 7) == 4 || e === 0) break;
			n.skip(e & 7);
		}
		return i;
	},
	create(e) {
		return Io.fromPartial(e ?? {});
	},
	fromPartial(e) {
		let t = Fo();
		return t.baseEvent = e.baseEvent !== void 0 && e.baseEvent !== null ? X.fromPartial(e.baseEvent) : void 0, t.stepName = e.stepName ?? "", t;
	}
};
function Lo() {
	return {
		baseEvent: void 0,
		stepName: ""
	};
}
var Ro = {
	encode(e, t = new K()) {
		return e.baseEvent !== void 0 && X.encode(e.baseEvent, t.uint32(10).fork()).join(), e.stepName !== "" && t.uint32(18).string(e.stepName), t;
	},
	decode(e, t) {
		let n = e instanceof q ? e : new q(e), r = t === void 0 ? n.len : n.pos + t, i = Lo();
		for (; n.pos < r;) {
			let e = n.uint32();
			switch (e >>> 3) {
				case 1:
					if (e !== 10) break;
					i.baseEvent = X.decode(n, n.uint32());
					continue;
				case 2:
					if (e !== 18) break;
					i.stepName = n.string();
					continue;
			}
			if ((e & 7) == 4 || e === 0) break;
			n.skip(e & 7);
		}
		return i;
	},
	create(e) {
		return Ro.fromPartial(e ?? {});
	},
	fromPartial(e) {
		let t = Lo();
		return t.baseEvent = e.baseEvent !== void 0 && e.baseEvent !== null ? X.fromPartial(e.baseEvent) : void 0, t.stepName = e.stepName ?? "", t;
	}
};
function zo() {
	return {
		baseEvent: void 0,
		messageId: void 0,
		role: void 0,
		delta: void 0,
		name: void 0
	};
}
var Bo = {
	encode(e, t = new K()) {
		return e.baseEvent !== void 0 && X.encode(e.baseEvent, t.uint32(10).fork()).join(), e.messageId !== void 0 && t.uint32(18).string(e.messageId), e.role !== void 0 && t.uint32(26).string(e.role), e.delta !== void 0 && t.uint32(34).string(e.delta), e.name !== void 0 && t.uint32(42).string(e.name), t;
	},
	decode(e, t) {
		let n = e instanceof q ? e : new q(e), r = t === void 0 ? n.len : n.pos + t, i = zo();
		for (; n.pos < r;) {
			let e = n.uint32();
			switch (e >>> 3) {
				case 1:
					if (e !== 10) break;
					i.baseEvent = X.decode(n, n.uint32());
					continue;
				case 2:
					if (e !== 18) break;
					i.messageId = n.string();
					continue;
				case 3:
					if (e !== 26) break;
					i.role = n.string();
					continue;
				case 4:
					if (e !== 34) break;
					i.delta = n.string();
					continue;
				case 5:
					if (e !== 42) break;
					i.name = n.string();
					continue;
			}
			if ((e & 7) == 4 || e === 0) break;
			n.skip(e & 7);
		}
		return i;
	},
	create(e) {
		return Bo.fromPartial(e ?? {});
	},
	fromPartial(e) {
		let t = zo();
		return t.baseEvent = e.baseEvent !== void 0 && e.baseEvent !== null ? X.fromPartial(e.baseEvent) : void 0, t.messageId = e.messageId ?? void 0, t.role = e.role ?? void 0, t.delta = e.delta ?? void 0, t.name = e.name ?? void 0, t;
	}
};
function Vo() {
	return {
		baseEvent: void 0,
		toolCallId: void 0,
		toolCallName: void 0,
		parentMessageId: void 0,
		delta: void 0
	};
}
var Ho = {
	encode(e, t = new K()) {
		return e.baseEvent !== void 0 && X.encode(e.baseEvent, t.uint32(10).fork()).join(), e.toolCallId !== void 0 && t.uint32(18).string(e.toolCallId), e.toolCallName !== void 0 && t.uint32(26).string(e.toolCallName), e.parentMessageId !== void 0 && t.uint32(34).string(e.parentMessageId), e.delta !== void 0 && t.uint32(42).string(e.delta), t;
	},
	decode(e, t) {
		let n = e instanceof q ? e : new q(e), r = t === void 0 ? n.len : n.pos + t, i = Vo();
		for (; n.pos < r;) {
			let e = n.uint32();
			switch (e >>> 3) {
				case 1:
					if (e !== 10) break;
					i.baseEvent = X.decode(n, n.uint32());
					continue;
				case 2:
					if (e !== 18) break;
					i.toolCallId = n.string();
					continue;
				case 3:
					if (e !== 26) break;
					i.toolCallName = n.string();
					continue;
				case 4:
					if (e !== 34) break;
					i.parentMessageId = n.string();
					continue;
				case 5:
					if (e !== 42) break;
					i.delta = n.string();
					continue;
			}
			if ((e & 7) == 4 || e === 0) break;
			n.skip(e & 7);
		}
		return i;
	},
	create(e) {
		return Ho.fromPartial(e ?? {});
	},
	fromPartial(e) {
		let t = Vo();
		return t.baseEvent = e.baseEvent !== void 0 && e.baseEvent !== null ? X.fromPartial(e.baseEvent) : void 0, t.toolCallId = e.toolCallId ?? void 0, t.toolCallName = e.toolCallName ?? void 0, t.parentMessageId = e.parentMessageId ?? void 0, t.delta = e.delta ?? void 0, t;
	}
};
function Uo() {
	return {
		textMessageStart: void 0,
		textMessageContent: void 0,
		textMessageEnd: void 0,
		toolCallStart: void 0,
		toolCallArgs: void 0,
		toolCallEnd: void 0,
		stateSnapshot: void 0,
		stateDelta: void 0,
		messagesSnapshot: void 0,
		raw: void 0,
		custom: void 0,
		runStarted: void 0,
		runFinished: void 0,
		runError: void 0,
		stepStarted: void 0,
		stepFinished: void 0,
		textMessageChunk: void 0,
		toolCallChunk: void 0
	};
}
var Wo = {
	encode(e, t = new K()) {
		return e.textMessageStart !== void 0 && so.encode(e.textMessageStart, t.uint32(10).fork()).join(), e.textMessageContent !== void 0 && lo.encode(e.textMessageContent, t.uint32(18).fork()).join(), e.textMessageEnd !== void 0 && fo.encode(e.textMessageEnd, t.uint32(26).fork()).join(), e.toolCallStart !== void 0 && mo.encode(e.toolCallStart, t.uint32(34).fork()).join(), e.toolCallArgs !== void 0 && go.encode(e.toolCallArgs, t.uint32(42).fork()).join(), e.toolCallEnd !== void 0 && vo.encode(e.toolCallEnd, t.uint32(50).fork()).join(), e.stateSnapshot !== void 0 && bo.encode(e.stateSnapshot, t.uint32(58).fork()).join(), e.stateDelta !== void 0 && So.encode(e.stateDelta, t.uint32(66).fork()).join(), e.messagesSnapshot !== void 0 && wo.encode(e.messagesSnapshot, t.uint32(74).fork()).join(), e.raw !== void 0 && Eo.encode(e.raw, t.uint32(82).fork()).join(), e.custom !== void 0 && Oo.encode(e.custom, t.uint32(90).fork()).join(), e.runStarted !== void 0 && Ao.encode(e.runStarted, t.uint32(98).fork()).join(), e.runFinished !== void 0 && Mo.encode(e.runFinished, t.uint32(106).fork()).join(), e.runError !== void 0 && Po.encode(e.runError, t.uint32(114).fork()).join(), e.stepStarted !== void 0 && Io.encode(e.stepStarted, t.uint32(122).fork()).join(), e.stepFinished !== void 0 && Ro.encode(e.stepFinished, t.uint32(130).fork()).join(), e.textMessageChunk !== void 0 && Bo.encode(e.textMessageChunk, t.uint32(138).fork()).join(), e.toolCallChunk !== void 0 && Ho.encode(e.toolCallChunk, t.uint32(146).fork()).join(), t;
	},
	decode(e, t) {
		let n = e instanceof q ? e : new q(e), r = t === void 0 ? n.len : n.pos + t, i = Uo();
		for (; n.pos < r;) {
			let e = n.uint32();
			switch (e >>> 3) {
				case 1:
					if (e !== 10) break;
					i.textMessageStart = so.decode(n, n.uint32());
					continue;
				case 2:
					if (e !== 18) break;
					i.textMessageContent = lo.decode(n, n.uint32());
					continue;
				case 3:
					if (e !== 26) break;
					i.textMessageEnd = fo.decode(n, n.uint32());
					continue;
				case 4:
					if (e !== 34) break;
					i.toolCallStart = mo.decode(n, n.uint32());
					continue;
				case 5:
					if (e !== 42) break;
					i.toolCallArgs = go.decode(n, n.uint32());
					continue;
				case 6:
					if (e !== 50) break;
					i.toolCallEnd = vo.decode(n, n.uint32());
					continue;
				case 7:
					if (e !== 58) break;
					i.stateSnapshot = bo.decode(n, n.uint32());
					continue;
				case 8:
					if (e !== 66) break;
					i.stateDelta = So.decode(n, n.uint32());
					continue;
				case 9:
					if (e !== 74) break;
					i.messagesSnapshot = wo.decode(n, n.uint32());
					continue;
				case 10:
					if (e !== 82) break;
					i.raw = Eo.decode(n, n.uint32());
					continue;
				case 11:
					if (e !== 90) break;
					i.custom = Oo.decode(n, n.uint32());
					continue;
				case 12:
					if (e !== 98) break;
					i.runStarted = Ao.decode(n, n.uint32());
					continue;
				case 13:
					if (e !== 106) break;
					i.runFinished = Mo.decode(n, n.uint32());
					continue;
				case 14:
					if (e !== 114) break;
					i.runError = Po.decode(n, n.uint32());
					continue;
				case 15:
					if (e !== 122) break;
					i.stepStarted = Io.decode(n, n.uint32());
					continue;
				case 16:
					if (e !== 130) break;
					i.stepFinished = Ro.decode(n, n.uint32());
					continue;
				case 17:
					if (e !== 138) break;
					i.textMessageChunk = Bo.decode(n, n.uint32());
					continue;
				case 18:
					if (e !== 146) break;
					i.toolCallChunk = Ho.decode(n, n.uint32());
					continue;
			}
			if ((e & 7) == 4 || e === 0) break;
			n.skip(e & 7);
		}
		return i;
	},
	create(e) {
		return Wo.fromPartial(e ?? {});
	},
	fromPartial(e) {
		let t = Uo();
		return t.textMessageStart = e.textMessageStart !== void 0 && e.textMessageStart !== null ? so.fromPartial(e.textMessageStart) : void 0, t.textMessageContent = e.textMessageContent !== void 0 && e.textMessageContent !== null ? lo.fromPartial(e.textMessageContent) : void 0, t.textMessageEnd = e.textMessageEnd !== void 0 && e.textMessageEnd !== null ? fo.fromPartial(e.textMessageEnd) : void 0, t.toolCallStart = e.toolCallStart !== void 0 && e.toolCallStart !== null ? mo.fromPartial(e.toolCallStart) : void 0, t.toolCallArgs = e.toolCallArgs !== void 0 && e.toolCallArgs !== null ? go.fromPartial(e.toolCallArgs) : void 0, t.toolCallEnd = e.toolCallEnd !== void 0 && e.toolCallEnd !== null ? vo.fromPartial(e.toolCallEnd) : void 0, t.stateSnapshot = e.stateSnapshot !== void 0 && e.stateSnapshot !== null ? bo.fromPartial(e.stateSnapshot) : void 0, t.stateDelta = e.stateDelta !== void 0 && e.stateDelta !== null ? So.fromPartial(e.stateDelta) : void 0, t.messagesSnapshot = e.messagesSnapshot !== void 0 && e.messagesSnapshot !== null ? wo.fromPartial(e.messagesSnapshot) : void 0, t.raw = e.raw !== void 0 && e.raw !== null ? Eo.fromPartial(e.raw) : void 0, t.custom = e.custom !== void 0 && e.custom !== null ? Oo.fromPartial(e.custom) : void 0, t.runStarted = e.runStarted !== void 0 && e.runStarted !== null ? Ao.fromPartial(e.runStarted) : void 0, t.runFinished = e.runFinished !== void 0 && e.runFinished !== null ? Mo.fromPartial(e.runFinished) : void 0, t.runError = e.runError !== void 0 && e.runError !== null ? Po.fromPartial(e.runError) : void 0, t.stepStarted = e.stepStarted !== void 0 && e.stepStarted !== null ? Io.fromPartial(e.stepStarted) : void 0, t.stepFinished = e.stepFinished !== void 0 && e.stepFinished !== null ? Ro.fromPartial(e.stepFinished) : void 0, t.textMessageChunk = e.textMessageChunk !== void 0 && e.textMessageChunk !== null ? Bo.fromPartial(e.textMessageChunk) : void 0, t.toolCallChunk = e.toolCallChunk !== void 0 && e.toolCallChunk !== null ? Ho.fromPartial(e.toolCallChunk) : void 0, t;
	}
};
function Go(e) {
	let t = globalThis.Number(e.toString());
	if (t > globalThis.Number.MAX_SAFE_INTEGER) throw new globalThis.Error("Value is larger than Number.MAX_SAFE_INTEGER");
	if (t < globalThis.Number.MIN_SAFE_INTEGER) throw new globalThis.Error("Value is smaller than Number.MIN_SAFE_INTEGER");
	return t;
}
var Ko = (e) => {
	if (!(!e || typeof e != "object")) {
		if (e.data) return {
			type: "data",
			value: e.data.value,
			mimeType: e.data.mimeType
		};
		if (e.url) return {
			type: "url",
			value: e.url.value,
			mimeType: e.url.mimeType
		};
	}
}, qo = (e) => {
	if (!(!e || typeof e != "object")) {
		if (e.text) return {
			type: "text",
			text: e.text.text
		};
		if (e.image) return {
			type: "image",
			source: Ko(e.image.source),
			metadata: e.image.metadata
		};
		if (e.audio) return {
			type: "audio",
			source: Ko(e.audio.source),
			metadata: e.audio.metadata
		};
		if (e.video) return {
			type: "video",
			source: Ko(e.video.source),
			metadata: e.video.metadata
		};
		if (e.document) return {
			type: "document",
			source: Ko(e.document.source),
			metadata: e.document.metadata
		};
	}
};
function Jo(e) {
	let t = Wo.decode(e), n = Object.values(t).find((e) => e !== void 0);
	if (!n) throw Error("Invalid event");
	if (n.type = io[n.baseEvent.type], n.timestamp = n.baseEvent.timestamp, n.rawEvent = n.baseEvent.rawEvent, n.type === F.MESSAGES_SNAPSHOT) for (let e of n.messages) {
		let t = e;
		if (t.role === "user" && Array.isArray(t.contentParts)) {
			let e = t.contentParts.map((e) => qo(e)).filter((e) => e !== void 0);
			e.length > 0 && (t.content = e);
		}
		Array.isArray(t.contentParts) && t.contentParts.length === 0 && (t.contentParts = void 0), t.toolCalls?.length === 0 && (t.toolCalls = void 0);
	}
	if (n.type === F.STATE_DELTA) for (let e of n.delta) e.op = Ma[e.op].toLowerCase(), Object.keys(e).forEach((t) => {
		e[t] === void 0 && delete e[t];
	});
	return Object.keys(n).forEach((e) => {
		n[e] === void 0 && delete n[e];
	}), An.parse(n);
}
//#endregion
//#region node_modules/compare-versions/lib/esm/utils.js
var Yo = /^[v^~<>=]*?(\d+)(?:\.([x*]|\d+)(?:\.([x*]|\d+)(?:\.([x*]|\d+))?(?:-([\da-z\-]+(?:\.[\da-z\-]+)*))?(?:\+[\da-z\-]+(?:\.[\da-z\-]+)*)?)?)?$/i, Xo = (e) => {
	if (typeof e != "string") throw TypeError("Invalid argument expected string");
	let t = e.match(Yo);
	if (!t) throw Error(`Invalid argument not valid semver ('${e}' received)`);
	return t.shift(), t;
}, Zo = (e) => e === "*" || e === "x" || e === "X", Qo = (e) => {
	let t = parseInt(e, 10);
	return isNaN(t) ? e : t;
}, $o = (e, t) => typeof e == typeof t ? [e, t] : [String(e), String(t)], es = (e, t) => {
	if (Zo(e) || Zo(t)) return 0;
	let [n, r] = $o(Qo(e), Qo(t));
	return n > r ? 1 : n < r ? -1 : 0;
}, ts = (e, t) => {
	for (let n = 0; n < Math.max(e.length, t.length); n++) {
		let r = es(e[n] || "0", t[n] || "0");
		if (r !== 0) return r;
	}
	return 0;
}, ns = (e, t) => {
	let n = Xo(e), r = Xo(t), i = n.pop(), a = r.pop(), o = ts(n, r);
	return o === 0 ? i && a ? ts(i.split("."), a.split(".")) : i || a ? i ? -1 : 1 : 0 : o;
}, Z = (e) => {
	if (typeof structuredClone == "function") return structuredClone(e);
	try {
		return JSON.parse(JSON.stringify(e));
	} catch {
		return Array.isArray(e) ? [...e] : { ...e };
	}
};
function rs() {
	return c();
}
function is(e) {
	if (Object.freeze(e), typeof e == "object" && e) for (let t of Object.values(e)) typeof t == "object" && t && !Object.isFrozen(t) && is(t);
	return e;
}
async function Q(e, t, n, r) {
	let i = typeof process < "u" && process.env !== void 0, a = i && (process.env.NODE_ENV === "test" || !!process.env.VITEST_WORKER_ID), o = i && (process.env.NODE_ENV === "development" || process.env.NODE_ENV === "test" || !!process.env.VITEST_WORKER_ID), s = Z(t), c = Z(n), l = s, u = c, d;
	for (let t of e) try {
		o && (is(l), is(u));
		let e = await r(t, l, u);
		if (e === void 0) continue;
		if (e.messages !== void 0 && e.messages !== l && (l = Z(e.messages)), e.state !== void 0 && e.state !== u && (u = Z(e.state)), d = e.stopPropagation, d === !0) break;
	} catch (e) {
		if (o && e instanceof TypeError) {
			if (a) throw e;
			console.error("AG-UI: Subscriber attempted to mutate frozen inputs in-place. Return mutations via AgentStateMutation instead of mutating directly.", e);
		} else a || console.error("Subscriber error:", e);
		continue;
	}
	return {
		...l === s ? {} : { messages: o && Object.isFrozen(l) ? Z(l) : l },
		...u === c ? {} : { state: o && Object.isFrozen(u) ? Z(u) : u },
		...d === void 0 ? {} : { stopPropagation: d }
	};
}
function as(e) {
	if (!e) return {
		enabled: !1,
		events: !1,
		lifecycle: !1,
		verbose: !1
	};
	if (e === !0) return {
		enabled: !0,
		events: !0,
		lifecycle: !0,
		verbose: !0
	};
	let t = e.events ?? !0, n = e.lifecycle ?? !0, r = e.verbose ?? !1;
	return {
		enabled: t || n,
		events: t,
		lifecycle: n,
		verbose: r
	};
}
function os(e) {
	if (e instanceof ss) return e;
	if (e === !0) return new ss(as(!0));
}
var ss = class {
	constructor(e) {
		this.config = e;
	}
	event(e, t, n, r) {
		this.config.events && (this.config.verbose ? console.debug(`[${e}] ${t}`, typeof n == "string" ? n : JSON.stringify(n)) : console.debug(`[${e}] ${t}`, r ?? n));
	}
	lifecycle(e, t, n) {
		this.config.lifecycle && (n ? console.debug(`[${e}] ${t}`, n) : console.debug(`[${e}] ${t}`));
	}
	get eventsEnabled() {
		return this.config.events;
	}
	get lifecycleEnabled() {
		return this.config.lifecycle;
	}
	get enabled() {
		return this.config.enabled;
	}
};
function cs(e) {
	return e.enabled ? new ss(e) : void 0;
}
function ls(e, t, n) {
	if (t) {
		let r = e.find((e) => e.id === t);
		if (r?.role === "assistant") return r;
		r && console.warn(`TOOL_CALL_START: parentMessageId '${t}' matches a '${r.role}' message, not assistant — falling back to toolCallId`);
		let i = {
			id: r ? n : t,
			role: "assistant",
			toolCalls: []
		};
		return e.push(i), i;
	}
	let r = {
		id: n,
		role: "assistant",
		toolCalls: []
	};
	return e.push(r), r;
}
var us = (e, t, n, r, i) => {
	let a = os(i), o = Z(n.messages), s = Z(e.state), c = {}, l = (e) => {
		e.messages !== void 0 && (o = e.messages, c.messages = e.messages), e.state !== void 0 && (s = e.state, c.state = e.state);
	}, u = () => {
		let e = Z(c);
		return c = {}, e.messages !== void 0 || e.state !== void 0 ? U(e) : ci;
	};
	return t.pipe(Yi(async (t) => {
		let i = await Q(r, o, s, (r, i, a) => r.onEvent?.({
			event: t,
			agent: n,
			input: e,
			messages: i,
			state: a
		}));
		if (l(i), i.stopPropagation === !0 ? a?.event("APPLY", "Event dropped:", t, {
			type: t.type,
			reason: "stopPropagation by subscriber"
		}) : a?.event("APPLY", "Event applied:", t, {
			type: t.type,
			subscribers: r.length
		}), i.stopPropagation === !0) return u();
		switch (t.type) {
			case F.TEXT_MESSAGE_START: {
				let i = await Q(r, o, s, (r, i, a) => r.onTextMessageStartEvent?.({
					event: t,
					messages: i,
					state: a,
					agent: n,
					input: e
				}));
				if (l(i), i.stopPropagation !== !0) {
					let { messageId: e, role: n = "assistant", name: r } = t;
					if (!o.find((t) => t.id === e)) {
						let t = {
							id: e,
							role: n,
							content: "",
							...r !== void 0 && { name: r }
						};
						o.push(t), l({ messages: o });
					}
				}
				return u();
			}
			case F.TEXT_MESSAGE_CONTENT: {
				let { messageId: i, delta: a } = t, c = o.find((e) => e.id === i);
				if (!c) return console.warn(`TEXT_MESSAGE_CONTENT: No message found with ID '${i}'`), u();
				let d = await Q(r, o, s, (r, i, a) => r.onTextMessageContentEvent?.({
					event: t,
					messages: i,
					state: a,
					agent: n,
					input: e,
					textMessageBuffer: typeof c.content == "string" ? c.content : ""
				}));
				return l(d), d.stopPropagation !== !0 && (c.content = `${typeof c.content == "string" ? c.content : ""}${a}`, l({ messages: o })), u();
			}
			case F.TEXT_MESSAGE_END: {
				let { messageId: i } = t, a = o.find((e) => e.id === i);
				return a ? (l(await Q(r, o, s, (r, i, o) => r.onTextMessageEndEvent?.({
					event: t,
					messages: i,
					state: o,
					agent: n,
					input: e,
					textMessageBuffer: typeof a.content == "string" ? a.content : ""
				}))), await Promise.all(r.map((t) => {
					t.onNewMessage?.({
						message: a,
						messages: o,
						state: s,
						agent: n,
						input: e
					});
				})), u()) : (console.warn(`TEXT_MESSAGE_END: No message found with ID '${i}'`), u());
			}
			case F.TOOL_CALL_START: {
				let i = await Q(r, o, s, (r, i, a) => r.onToolCallStartEvent?.({
					event: t,
					messages: i,
					state: a,
					agent: n,
					input: e
				}));
				if (l(i), i.stopPropagation !== !0) {
					let { toolCallId: e, toolCallName: n, parentMessageId: r } = t, i = ls(o, r, e);
					i.toolCalls ??= [], i.toolCalls.push({
						id: e,
						type: "function",
						function: {
							name: n,
							arguments: ""
						}
					}), l({ messages: o });
				}
				return u();
			}
			case F.TOOL_CALL_ARGS: {
				let { toolCallId: i, delta: a } = t, c = o.find((e) => e.toolCalls?.some((e) => e.id === i));
				if (!c) return console.warn(`TOOL_CALL_ARGS: No message found containing tool call with ID '${i}'`), u();
				let d = c.toolCalls?.find((e) => e.id === i);
				if (!d) return console.warn(`TOOL_CALL_ARGS: No tool call found with ID '${i}'`), u();
				let f = await Q(r, o, s, (r, i, a) => {
					let o = d.function.arguments, s = d.function.name, c = {};
					try {
						c = na(o);
					} catch {}
					return r.onToolCallArgsEvent?.({
						event: t,
						messages: i,
						state: a,
						agent: n,
						input: e,
						toolCallBuffer: o,
						toolCallName: s,
						partialToolCallArgs: c
					});
				});
				return l(f), f.stopPropagation !== !0 && (d.function.arguments += a, l({ messages: o })), u();
			}
			case F.TOOL_CALL_END: {
				let { toolCallId: i } = t, a = o.find((e) => e.toolCalls?.some((e) => e.id === i));
				if (!a) return console.warn(`TOOL_CALL_END: No message found containing tool call with ID '${i}'`), u();
				let c = a.toolCalls?.find((e) => e.id === i);
				return c ? (l(await Q(r, o, s, (r, i, a) => {
					let o = c.function.arguments, s = c.function.name, l = {};
					try {
						l = JSON.parse(o);
					} catch {}
					return r.onToolCallEndEvent?.({
						event: t,
						messages: i,
						state: a,
						agent: n,
						input: e,
						toolCallName: s,
						toolCallArgs: l
					});
				})), await Promise.all(r.map((t) => {
					t.onNewToolCall?.({
						toolCall: c,
						messages: o,
						state: s,
						agent: n,
						input: e
					});
				})), u()) : (console.warn(`TOOL_CALL_END: No tool call found with ID '${i}'`), u());
			}
			case F.TOOL_CALL_RESULT: {
				let i = await Q(r, o, s, (r, i, a) => r.onToolCallResultEvent?.({
					event: t,
					messages: i,
					state: a,
					agent: n,
					input: e
				}));
				if (l(i), i.stopPropagation !== !0) {
					let { messageId: i, toolCallId: a, content: c, role: u } = t, d = {
						id: i,
						toolCallId: a,
						role: u || "tool",
						content: c
					};
					o.push(d), await Promise.all(r.map((t) => {
						t.onNewMessage?.({
							message: d,
							messages: o,
							state: s,
							agent: n,
							input: e
						});
					})), l({ messages: o });
				}
				return u();
			}
			case F.STATE_SNAPSHOT: {
				let i = await Q(r, o, s, (r, i, a) => r.onStateSnapshotEvent?.({
					event: t,
					messages: i,
					state: a,
					agent: n,
					input: e
				}));
				if (l(i), i.stopPropagation !== !0) {
					let { snapshot: e } = t;
					s = e, l({ state: s });
				}
				return u();
			}
			case F.STATE_DELTA: {
				let i = await Q(r, o, s, (r, i, a) => r.onStateDeltaEvent?.({
					event: t,
					messages: i,
					state: a,
					agent: n,
					input: e
				}));
				if (l(i), i.stopPropagation !== !0) {
					let { delta: e } = t;
					try {
						s = ur.applyPatch(s, e, !0, !1).newDocument, l({ state: s });
					} catch (t) {
						let n = t instanceof Error ? t.message : String(t);
						console.warn(`Failed to apply state patch:\nCurrent state: ${JSON.stringify(s, null, 2)}\nPatch operations: ${JSON.stringify(e, null, 2)}\nError: ${n}`);
					}
				}
				return u();
			}
			case F.MESSAGES_SNAPSHOT: {
				let i = await Q(r, o, s, (r, i, a) => r.onMessagesSnapshotEvent?.({
					event: t,
					messages: i,
					state: a,
					agent: n,
					input: e
				}));
				if (l(i), i.stopPropagation !== !0) {
					let { messages: e } = t, n = new Map(e.map((e) => [e.id, e])), r = (e) => e === "activity" || e === "reasoning";
					o = o.filter((e) => r(e.role) || n.has(e.id)).map((e) => r(e.role) ? e : n.get(e.id));
					let i = new Set(o.map((e) => e.id));
					for (let t of e) i.has(t.id) || o.push(t);
					l({ messages: o });
				}
				return u();
			}
			case F.ACTIVITY_SNAPSHOT: {
				let i = t, a = o.findIndex((e) => e.id === i.messageId), c = a >= 0 ? o[a] : void 0, d = c?.role === "activity" ? c : void 0, f = i.replace ?? !0, p = await Q(r, o, s, (t, r, a) => t.onActivitySnapshotEvent?.({
					event: i,
					messages: r,
					state: a,
					agent: n,
					input: e,
					activityMessage: d,
					existingMessage: c
				}));
				if (l(p), p.stopPropagation !== !0) {
					let t = {
						id: i.messageId,
						role: "activity",
						activityType: i.activityType,
						content: Z(i.content)
					}, c;
					a === -1 ? (o.push(t), c = t) : d ? f && (o[a] = {
						...d,
						activityType: i.activityType,
						content: Z(i.content)
					}) : f && (o[a] = t, c = t), l({ messages: o }), c && await Promise.all(r.map((t) => t.onNewMessage?.({
						message: c,
						messages: o,
						state: s,
						agent: n,
						input: e
					})));
				}
				return u();
			}
			case F.ACTIVITY_DELTA: {
				let i = t, a = o.findIndex((e) => e.id === i.messageId);
				if (a === -1) return u();
				let c = o[a];
				if (c.role !== "activity") return console.warn(`ACTIVITY_DELTA: Message '${i.messageId}' is not an activity message`), u();
				let d = c, f = await Q(r, o, s, (t, r, a) => t.onActivityDeltaEvent?.({
					event: i,
					messages: r,
					state: a,
					agent: n,
					input: e,
					activityMessage: d
				}));
				if (l(f), f.stopPropagation !== !0) try {
					let e = Z(d.content ?? {}), t = ur.applyPatch(e, i.patch ?? [], !0, !1).newDocument;
					o[a] = {
						...d,
						content: Z(t),
						activityType: i.activityType
					}, l({ messages: o });
				} catch (e) {
					let t = e instanceof Error ? e.message : String(e);
					console.warn(`Failed to apply activity patch for '${i.messageId}': ${t}`);
				}
				return u();
			}
			case F.RAW: return l(await Q(r, o, s, (r, i, a) => r.onRawEvent?.({
				event: t,
				messages: i,
				state: a,
				agent: n,
				input: e
			}))), u();
			case F.CUSTOM: return l(await Q(r, o, s, (r, i, a) => r.onCustomEvent?.({
				event: t,
				messages: i,
				state: a,
				agent: n,
				input: e
			}))), u();
			case F.RUN_STARTED: {
				let i = await Q(r, o, s, (r, i, a) => r.onRunStartedEvent?.({
					event: t,
					messages: i,
					state: a,
					agent: n,
					input: e
				}));
				if (l(i), i.stopPropagation !== !0) {
					let e = t;
					if (e.input?.messages) {
						for (let t of e.input.messages) o.find((e) => e.id === t.id) || o.push(t);
						l({ messages: o });
					}
				}
				return u();
			}
			case F.RUN_FINISHED: return l(await Q(r, o, s, (r, i, a) => r.onRunFinishedEvent?.({
				event: t,
				messages: i,
				state: a,
				agent: n,
				input: e,
				result: t.result
			}))), u();
			case F.RUN_ERROR: return l(await Q(r, o, s, (r, i, a) => r.onRunErrorEvent?.({
				event: t,
				messages: i,
				state: a,
				agent: n,
				input: e
			}))), u();
			case F.STEP_STARTED: return l(await Q(r, o, s, (r, i, a) => r.onStepStartedEvent?.({
				event: t,
				messages: i,
				state: a,
				agent: n,
				input: e
			}))), u();
			case F.STEP_FINISHED: return l(await Q(r, o, s, (r, i, a) => r.onStepFinishedEvent?.({
				event: t,
				messages: i,
				state: a,
				agent: n,
				input: e
			}))), u();
			case F.TEXT_MESSAGE_CHUNK: throw Error("TEXT_MESSAGE_CHUNK must be tranformed before being applied");
			case F.TOOL_CALL_CHUNK: throw Error("TOOL_CALL_CHUNK must be tranformed before being applied");
			case F.THINKING_START: return u();
			case F.THINKING_END: return u();
			case F.THINKING_TEXT_MESSAGE_START: return u();
			case F.THINKING_TEXT_MESSAGE_CONTENT: return u();
			case F.THINKING_TEXT_MESSAGE_END: return u();
			case F.REASONING_START: return l(await Q(r, o, s, (r, i, a) => r.onReasoningStartEvent?.({
				event: t,
				messages: i,
				state: a,
				agent: n,
				input: e
			}))), u();
			case F.REASONING_MESSAGE_START: {
				let i = await Q(r, o, s, (r, i, a) => r.onReasoningMessageStartEvent?.({
					event: t,
					messages: i,
					state: a,
					agent: n,
					input: e
				}));
				if (l(i), i.stopPropagation !== !0) {
					let { messageId: e } = t;
					if (!o.find((t) => t.id === e)) {
						let t = {
							id: e,
							role: "reasoning",
							content: ""
						};
						o.push(t), l({ messages: o });
					}
				}
				return u();
			}
			case F.REASONING_MESSAGE_CONTENT: {
				let { messageId: i, delta: a } = t, c = o.find((e) => e.id === i);
				if (!c) return console.warn(`REASONING_MESSAGE_CONTENT: No message found with ID '${i}'`), u();
				let d = await Q(r, o, s, (r, i, a) => r.onReasoningMessageContentEvent?.({
					event: t,
					messages: i,
					state: a,
					agent: n,
					input: e,
					reasoningMessageBuffer: typeof c.content == "string" ? c.content : ""
				}));
				return l(d), d.stopPropagation !== !0 && (c.content = `${typeof c.content == "string" ? c.content : ""}${a}`, l({ messages: o })), u();
			}
			case F.REASONING_MESSAGE_END: {
				let { messageId: i } = t, a = o.find((e) => e.id === i);
				return a ? (l(await Q(r, o, s, (r, i, o) => r.onReasoningMessageEndEvent?.({
					event: t,
					messages: i,
					state: o,
					agent: n,
					input: e,
					reasoningMessageBuffer: typeof a.content == "string" ? a.content : ""
				}))), await Promise.all(r.map((t) => {
					t.onNewMessage?.({
						message: a,
						messages: o,
						state: s,
						agent: n,
						input: e
					});
				})), u()) : (console.warn(`REASONING_MESSAGE_END: No message found with ID '${i}'`), u());
			}
			case F.REASONING_MESSAGE_CHUNK: throw Error("REASONING_MESSAGE_CHUNK must be transformed before being applied");
			case F.REASONING_END: return l(await Q(r, o, s, (r, i, a) => r.onReasoningEndEvent?.({
				event: t,
				messages: i,
				state: a,
				agent: n,
				input: e
			}))), u();
			case F.REASONING_ENCRYPTED_VALUE: {
				let { subtype: i, entityId: a, encryptedValue: d } = t, f = await Q(r, o, s, (r, i, a) => r.onReasoningEncryptedValueEvent?.({
					event: t,
					messages: i,
					state: a,
					agent: n,
					input: e
				}));
				if (l(f), f.stopPropagation !== !0) {
					let e = !1;
					if (i === "tool-call") {
						for (let t of o) if (t.role === "assistant" && t.toolCalls) {
							let n = t.toolCalls.find((e) => e.id === a);
							if (n) {
								n.encryptedValue = d, e = !0;
								break;
							}
						}
					} else {
						let t = o.find((e) => e.id === a);
						t?.role !== "activity" && t && (t.encryptedValue = d, e = !0);
					}
					e && (c.messages = o);
				}
				return u();
			}
		}
		return t.type, u();
	}), Ki(), r.length > 0 ? Xi({}) : (e) => e);
}, ds = (e) => (t) => {
	let n = os(e), r = /* @__PURE__ */ new Map(), i = /* @__PURE__ */ new Map(), a = !1, o = !1, s = !1, c = /* @__PURE__ */ new Map(), l = !1, u = !1, d = !1, f = () => {
		r.clear(), i.clear(), c.clear(), l = !1, u = !1, a = !1, o = !1, d = !0;
	};
	return t.pipe(Gi((e) => {
		let t = e.type;
		if (n?.event("VERIFY", "Event:", e, { type: e.type }), o) return W(() => new P(`Cannot send event type '${t}': The run has already errored with 'RUN_ERROR'. No further events can be sent.`));
		if (a && t !== F.RUN_ERROR && t !== F.RUN_STARTED) return W(() => new P(`Cannot send event type '${t}': The run has already finished with 'RUN_FINISHED'. Start a new run with 'RUN_STARTED'.`));
		if (!s) {
			if (s = !0, t !== F.RUN_STARTED && t !== F.RUN_ERROR) return W(() => new P("First event must be 'RUN_STARTED'"));
		} else if (t === F.RUN_STARTED) {
			if (d && !a) return W(() => new P("Cannot send 'RUN_STARTED' while a run is still active. The previous run must be finished with 'RUN_FINISHED' before starting a new run."));
			a && f();
		}
		switch (t) {
			case F.TEXT_MESSAGE_START: {
				let t = e.messageId;
				return r.has(t) ? W(() => new P(`Cannot send 'TEXT_MESSAGE_START' event: A text message with ID '${t}' is already in progress. Complete it with 'TEXT_MESSAGE_END' first.`)) : (r.set(t, !0), U(e));
			}
			case F.TEXT_MESSAGE_CONTENT: {
				let t = e.messageId;
				return r.has(t) ? U(e) : W(() => new P(`Cannot send 'TEXT_MESSAGE_CONTENT' event: No active text message found with ID '${t}'. Start a text message with 'TEXT_MESSAGE_START' first.`));
			}
			case F.TEXT_MESSAGE_END: {
				let t = e.messageId;
				return r.has(t) ? (r.delete(t), U(e)) : W(() => new P(`Cannot send 'TEXT_MESSAGE_END' event: No active text message found with ID '${t}'. A 'TEXT_MESSAGE_START' event must be sent first.`));
			}
			case F.TOOL_CALL_START: {
				let t = e.toolCallId;
				return i.has(t) ? W(() => new P(`Cannot send 'TOOL_CALL_START' event: A tool call with ID '${t}' is already in progress. Complete it with 'TOOL_CALL_END' first.`)) : (i.set(t, !0), U(e));
			}
			case F.TOOL_CALL_ARGS: {
				let t = e.toolCallId;
				return i.has(t) ? U(e) : W(() => new P(`Cannot send 'TOOL_CALL_ARGS' event: No active tool call found with ID '${t}'. Start a tool call with 'TOOL_CALL_START' first.`));
			}
			case F.TOOL_CALL_END: {
				let t = e.toolCallId;
				return i.has(t) ? (i.delete(t), U(e)) : W(() => new P(`Cannot send 'TOOL_CALL_END' event: No active tool call found with ID '${t}'. A 'TOOL_CALL_START' event must be sent first.`));
			}
			case F.STEP_STARTED: {
				let t = e.stepName;
				return c.has(t) ? W(() => new P(`Step "${t}" is already active for 'STEP_STARTED'`)) : (c.set(t, !0), U(e));
			}
			case F.STEP_FINISHED: {
				let t = e.stepName;
				return c.has(t) ? (c.delete(t), U(e)) : W(() => new P(`Cannot send 'STEP_FINISHED' for step "${t}" that was not started`));
			}
			case F.RUN_STARTED: return d = !0, U(e);
			case F.RUN_FINISHED:
				if (c.size > 0) {
					let e = Array.from(c.keys()).join(", ");
					return W(() => new P(`Cannot send 'RUN_FINISHED' while steps are still active: ${e}`));
				}
				if (r.size > 0) {
					let e = Array.from(r.keys()).join(", ");
					return W(() => new P(`Cannot send 'RUN_FINISHED' while text messages are still active: ${e}`));
				}
				if (i.size > 0) {
					let e = Array.from(i.keys()).join(", ");
					return W(() => new P(`Cannot send 'RUN_FINISHED' while tool calls are still active: ${e}`));
				}
				return a = !0, U(e);
			case F.RUN_ERROR: return o = !0, U(e);
			case F.CUSTOM: return U(e);
			case F.THINKING_TEXT_MESSAGE_START: return l ? u ? W(() => new P("Cannot send 'THINKING_TEXT_MESSAGE_START' event: A thinking message is already in progress. Complete it with 'THINKING_TEXT_MESSAGE_END' first.")) : (u = !0, U(e)) : W(() => new P("Cannot send 'THINKING_TEXT_MESSAGE_START' event: A thinking step is not in progress. Create one with 'THINKING_START' first."));
			case F.THINKING_TEXT_MESSAGE_CONTENT: return u ? U(e) : W(() => new P("Cannot send 'THINKING_TEXT_MESSAGE_CONTENT' event: No active thinking message found. Start a message with 'THINKING_TEXT_MESSAGE_START' first."));
			case F.THINKING_TEXT_MESSAGE_END: return u ? (u = !1, U(e)) : W(() => new P("Cannot send 'THINKING_TEXT_MESSAGE_END' event: No active thinking message found. A 'THINKING_TEXT_MESSAGE_START' event must be sent first."));
			case F.THINKING_START: return l ? W(() => new P("Cannot send 'THINKING_START' event: A thinking step is already in progress. End it with 'THINKING_END' first.")) : (l = !0, U(e));
			case F.THINKING_END: return l ? (l = !1, U(e)) : W(() => new P("Cannot send 'THINKING_END' event: No active thinking step found. A 'THINKING_START' event must be sent first."));
			default: return U(e);
		}
	}));
}, fs = function(e) {
	return e.HEADERS = "headers", e.DATA = "data", e;
}({}), ps = (e, t) => qi(() => Bi(fetch(e, t))).pipe(Qi((e) => {
	if (!e.ok) {
		let t = e.headers.get("content-type") || "";
		return Bi(e.text()).pipe(Gi((n) => {
			let r = n;
			if (t.includes("application/json")) try {
				r = JSON.parse(n);
			} catch {}
			let i = Error(`HTTP ${e.status}: ${typeof r == "string" ? r : JSON.stringify(r)}`);
			return i.status = e.status, i.payload = r, W(() => i);
		}));
	}
	let t = {
		type: fs.HEADERS,
		status: e.status,
		headers: e.headers
	}, n = e.body?.getReader();
	return n ? new B((e) => (e.next(t), (async () => {
		try {
			for (;;) {
				let { done: t, value: r } = await n.read();
				if (t) break;
				let i = {
					type: fs.DATA,
					data: r
				};
				e.next(i);
			}
			e.complete();
		} catch (t) {
			e.error(t);
		}
	})(), () => {
		n.cancel().catch((e) => {
			if (e?.name !== "AbortError") throw e;
		});
	})) : W(() => Error("Failed to getReader() from response"));
})), ms = (e, t) => {
	let n = os(t), r = new ii(), i = new TextDecoder("utf-8", { fatal: !1 }), a = "";
	e.subscribe({
		next: (e) => {
			if (e.type !== fs.HEADERS && e.type === fs.DATA && e.data) {
				let t = i.decode(e.data, { stream: !0 });
				a += t;
				let n = a.split(/\n\n/);
				a = n.pop() || "";
				for (let e of n) o(e);
			}
		},
		error: (e) => r.error(e),
		complete: () => {
			a && (a += i.decode(), o(a)), r.complete();
		}
	});
	function o(e) {
		let t = e.split("\n"), i = [];
		for (let e of t) e.startsWith("data:") && i.push(e.slice(5).replace(/^ /, ""));
		if (i.length > 0) try {
			let e = i.join("\n"), t = JSON.parse(e);
			n?.event("SSE", "Event received:", t, { type: t.type }), r.next(t);
		} catch (e) {
			r.error(e);
		}
	}
	return r.asObservable();
}, hs = (e) => {
	let t = new ii(), n = new Uint8Array();
	e.subscribe({
		next: (e) => {
			if (e.type !== fs.HEADERS && e.type === fs.DATA && e.data) {
				let t = new Uint8Array(n.length + e.data.length);
				t.set(n, 0), t.set(e.data, n.length), n = t, r();
			}
		},
		error: (e) => t.error(e),
		complete: () => {
			if (n.length > 0) try {
				r();
			} catch {
				console.warn("Incomplete or invalid protocol buffer data at stream end");
			}
			t.complete();
		}
	});
	function r() {
		for (; n.length >= 4;) {
			let e = 4 + new DataView(n.buffer, n.byteOffset, 4).getUint32(0, !1);
			if (n.length < e) break;
			try {
				let r = Jo(n.slice(4, e));
				t.next(r), n = n.slice(e);
			} catch (e) {
				let n = e instanceof Error ? e.message : String(e);
				t.error(Error(`Failed to decode protocol buffer message: ${n}`));
				return;
			}
		}
	}
	return t.asObservable();
}, gs = (e, t) => {
	let n = os(t), r = new ii(), i = new si(), a = !1;
	return e.subscribe({
		next: (e) => {
			if (i.next(e), e.type === fs.HEADERS && !a) {
				a = !0;
				let t = e.headers.get("content-type");
				n?.lifecycle("HTTP", "Stream format detected:", {
					contentType: t,
					parser: t === "application/vnd.ag-ui.event+proto" ? "protobuf" : "sse"
				}), t === "application/vnd.ag-ui.event+proto" ? hs(i).subscribe({
					next: (e) => r.next(e),
					error: (e) => r.error(e),
					complete: () => r.complete()
				}) : ms(i, n).subscribe({
					next: (e) => {
						try {
							let t = An.parse(e);
							n?.event("HTTP", "Event validated:", t, {
								type: t.type,
								valid: !0
							}), r.next(t);
						} catch (t) {
							n?.event("HTTP", "Event invalid:", {
								json: e,
								error: String(t)
							}), r.error(t);
						}
					},
					error: (e) => {
						if (e?.name === "AbortError") {
							r.next({
								type: F.RUN_ERROR,
								message: e.message || "Request aborted",
								code: "abort",
								rawEvent: e
							}), r.complete();
							return;
						}
						return r.error(e);
					},
					complete: () => r.complete()
				});
			} else a || r.error(Error("No headers event received before data events"));
		},
		error: (e) => {
			i.error(e), r.error(e);
		},
		complete: () => {
			i.complete();
		}
	}), r.asObservable();
}, $ = St([
	"TextMessageStart",
	"TextMessageContent",
	"TextMessageEnd",
	"ActionExecutionStart",
	"ActionExecutionArgs",
	"ActionExecutionEnd",
	"ActionExecutionResult",
	"AgentStateMessage",
	"MetaEvent",
	"RunStarted",
	"RunFinished",
	"RunError",
	"NodeStarted",
	"NodeFinished"
]), _s = St([
	"LangGraphInterruptEvent",
	"PredictState",
	"Exit"
]);
bt("type", [
	M({
		type: N($.enum.TextMessageStart),
		messageId: O(),
		parentMessageId: O().optional(),
		role: O().optional()
	}),
	M({
		type: N($.enum.TextMessageContent),
		messageId: O(),
		content: O()
	}),
	M({
		type: N($.enum.TextMessageEnd),
		messageId: O()
	}),
	M({
		type: N($.enum.ActionExecutionStart),
		actionExecutionId: O(),
		actionName: O(),
		parentMessageId: O().optional()
	}),
	M({
		type: N($.enum.ActionExecutionArgs),
		actionExecutionId: O(),
		args: O()
	}),
	M({
		type: N($.enum.ActionExecutionEnd),
		actionExecutionId: O()
	}),
	M({
		type: N($.enum.ActionExecutionResult),
		actionName: O(),
		actionExecutionId: O(),
		result: O()
	}),
	M({
		type: N($.enum.AgentStateMessage),
		threadId: O(),
		agentName: O(),
		nodeName: O(),
		runId: O(),
		active: k(),
		role: O(),
		state: O(),
		running: k()
	}),
	M({
		type: N($.enum.MetaEvent),
		name: _s,
		value: A()
	}),
	M({
		type: N($.enum.RunError),
		message: O(),
		code: O().optional()
	})
]), M({
	id: O(),
	role: O(),
	content: O(),
	parentMessageId: O().optional()
}), M({
	id: O(),
	name: O(),
	arguments: A(),
	parentMessageId: O().optional()
}), M({
	id: O(),
	result: A(),
	actionExecutionId: O(),
	actionName: O()
});
var vs = (e) => {
	if (typeof e == "string") return e;
	if (!Array.isArray(e)) return;
	let t = e.filter((e) => e.type === "text").map((e) => e.text).filter((e) => e.length > 0);
	if (t.length !== 0) return t.join("\n");
}, ys = (e, t, n) => (r) => {
	let i = {}, a = !0, o = !0, s = "", c = null, l = null, u = [], d = {}, f = (e) => {
		typeof e == "object" && e && ("messages" in e && delete e.messages, i = e);
	};
	return r.pipe(Gi((r) => {
		switch (r.type) {
			case F.TEXT_MESSAGE_START: {
				let e = r;
				return [{
					type: $.enum.TextMessageStart,
					messageId: e.messageId,
					role: e.role
				}];
			}
			case F.TEXT_MESSAGE_CONTENT: {
				let e = r;
				return [{
					type: $.enum.TextMessageContent,
					messageId: e.messageId,
					content: e.delta
				}];
			}
			case F.TEXT_MESSAGE_END: {
				let e = r;
				return [{
					type: $.enum.TextMessageEnd,
					messageId: e.messageId
				}];
			}
			case F.TOOL_CALL_START: {
				let e = r;
				return u.push({
					id: e.toolCallId,
					type: "function",
					function: {
						name: e.toolCallName,
						arguments: ""
					}
				}), o = !0, d[e.toolCallId] = e.toolCallName, [{
					type: $.enum.ActionExecutionStart,
					actionExecutionId: e.toolCallId,
					actionName: e.toolCallName,
					parentMessageId: e.parentMessageId
				}];
			}
			case F.TOOL_CALL_ARGS: {
				let c = r, d = u.find((e) => e.id === c.toolCallId);
				if (!d) return console.warn(`TOOL_CALL_ARGS: No tool call found with ID '${c.toolCallId}'`), [];
				d.function.arguments += c.delta;
				let p = !1;
				if (l) {
					let e = l.find((e) => e.tool == d.function.name);
					if (e) try {
						let t = JSON.parse(na(d.function.arguments));
						e.tool_argument && e.tool_argument in t ? (f({
							...i,
							[e.state_key]: t[e.tool_argument]
						}), p = !0) : e.tool_argument || (f({
							...i,
							[e.state_key]: t
						}), p = !0);
					} catch {}
				}
				return [{
					type: $.enum.ActionExecutionArgs,
					actionExecutionId: c.toolCallId,
					args: c.delta
				}, ...p ? [{
					type: $.enum.AgentStateMessage,
					threadId: e,
					agentName: n,
					nodeName: s,
					runId: t,
					running: a,
					role: "assistant",
					state: JSON.stringify(i),
					active: o
				}] : []];
			}
			case F.TOOL_CALL_END: {
				let e = r;
				return [{
					type: $.enum.ActionExecutionEnd,
					actionExecutionId: e.toolCallId
				}];
			}
			case F.TOOL_CALL_RESULT: {
				let e = r;
				return [{
					type: $.enum.ActionExecutionResult,
					actionExecutionId: e.toolCallId,
					result: e.content,
					actionName: d[e.toolCallId] || "unknown"
				}];
			}
			case F.RAW: return [];
			case F.CUSTOM: {
				let e = r;
				switch (e.name) {
					case "Exit":
						a = !1;
						break;
					case "PredictState":
						l = e.value;
						break;
				}
				return [{
					type: $.enum.MetaEvent,
					name: e.name,
					value: e.value
				}];
			}
			case F.STATE_SNAPSHOT: return f(r.snapshot), [{
				type: $.enum.AgentStateMessage,
				threadId: e,
				agentName: n,
				nodeName: s,
				runId: t,
				running: a,
				role: "assistant",
				state: JSON.stringify(i),
				active: o
			}];
			case F.STATE_DELTA: {
				let c = r, l = ur.applyPatch(i, c.delta, !0, !1);
				return l ? (f(l.newDocument), [{
					type: $.enum.AgentStateMessage,
					threadId: e,
					agentName: n,
					nodeName: s,
					runId: t,
					running: a,
					role: "assistant",
					state: JSON.stringify(i),
					active: o
				}]) : [];
			}
			case F.MESSAGES_SNAPSHOT: return c = r.messages, [{
				type: $.enum.AgentStateMessage,
				threadId: e,
				agentName: n,
				nodeName: s,
				runId: t,
				running: a,
				role: "assistant",
				state: JSON.stringify({
					...i,
					...c ? { messages: c } : {}
				}),
				active: !0
			}];
			case F.RUN_STARTED: return [];
			case F.RUN_FINISHED: return c && (i.messages = c), Object.keys(i).length === 0 ? [] : [{
				type: $.enum.AgentStateMessage,
				threadId: e,
				agentName: n,
				nodeName: s,
				runId: t,
				running: a,
				role: "assistant",
				state: JSON.stringify({
					...i,
					...c ? { messages: bs(c) } : {}
				}),
				active: !1
			}];
			case F.RUN_ERROR: {
				let e = r;
				return [{
					type: $.enum.RunError,
					message: e.message,
					code: e.code
				}];
			}
			case F.STEP_STARTED: return s = r.stepName, u = [], l = null, [{
				type: $.enum.AgentStateMessage,
				threadId: e,
				agentName: n,
				nodeName: s,
				runId: t,
				running: a,
				role: "assistant",
				state: JSON.stringify(i),
				active: !0
			}];
			case F.STEP_FINISHED: return u = [], l = null, [{
				type: $.enum.AgentStateMessage,
				threadId: e,
				agentName: n,
				nodeName: s,
				runId: t,
				running: a,
				role: "assistant",
				state: JSON.stringify(i),
				active: !1
			}];
			default: return [];
		}
	}));
};
function bs(e) {
	let t = [];
	for (let n of e) if (n.role === "assistant" || n.role === "user" || n.role === "system") {
		let e = vs(n.content);
		if (e) {
			let r = {
				id: n.id,
				role: n.role,
				content: e
			};
			t.push(r);
		}
		if (n.role === "assistant" && n.toolCalls && n.toolCalls.length > 0) for (let e of n.toolCalls) {
			let r = {
				id: e.id,
				name: e.function.name,
				arguments: JSON.parse(e.function.arguments),
				parentMessageId: n.id
			};
			t.push(r);
		}
	} else if (n.role === "tool") {
		let r = "unknown";
		for (let t of e) if (t.role === "assistant" && t.toolCalls?.length) {
			for (let e of t.toolCalls) if (e.id === n.toolCallId) {
				r = e.function.name;
				break;
			}
		}
		let i = {
			id: n.id,
			result: n.content,
			actionExecutionId: n.toolCallId,
			actionName: r
		};
		t.push(i);
	}
	return t;
}
var xs = (e) => (t) => {
	let n = os(e), r, i, a, o, s = () => {
		if (!r || o !== "text") throw Error("No text message to close");
		let e = {
			type: F.TEXT_MESSAGE_END,
			messageId: r.messageId
		};
		return o = void 0, r = void 0, n?.event("TRANSFORM", "TEXT_MESSAGE_END", e, { messageId: e.messageId }), e;
	}, c = () => {
		if (!i || o !== "tool") throw Error("No tool call to close");
		let e = {
			type: F.TOOL_CALL_END,
			toolCallId: i.toolCallId
		};
		return o = void 0, i = void 0, n?.event("TRANSFORM", "TOOL_CALL_END", e, { toolCallId: e.toolCallId }), e;
	}, l = () => {
		if (!a || o !== "reasoning") throw Error("No reasoning message to close");
		let e = {
			type: F.REASONING_MESSAGE_END,
			messageId: a.messageId
		};
		return o = void 0, a = void 0, n?.event("TRANSFORM", "REASONING_MESSAGE_END", e, { messageId: e.messageId }), e;
	}, u = () => o === "text" ? [s()] : o === "tool" ? [c()] : o === "reasoning" ? [l()] : [];
	return t.pipe(Gi((e) => {
		switch (e.type) {
			case F.TEXT_MESSAGE_START:
			case F.TEXT_MESSAGE_CONTENT:
			case F.TEXT_MESSAGE_END:
			case F.TOOL_CALL_START:
			case F.TOOL_CALL_ARGS:
			case F.TOOL_CALL_END:
			case F.TOOL_CALL_RESULT:
			case F.STATE_SNAPSHOT:
			case F.STATE_DELTA:
			case F.MESSAGES_SNAPSHOT:
			case F.CUSTOM:
			case F.RUN_STARTED:
			case F.RUN_FINISHED:
			case F.RUN_ERROR:
			case F.STEP_STARTED:
			case F.STEP_FINISHED:
			case F.THINKING_START:
			case F.THINKING_END:
			case F.THINKING_TEXT_MESSAGE_START:
			case F.THINKING_TEXT_MESSAGE_CONTENT:
			case F.THINKING_TEXT_MESSAGE_END:
			case F.REASONING_START:
			case F.REASONING_MESSAGE_START:
			case F.REASONING_MESSAGE_CONTENT:
			case F.REASONING_MESSAGE_END:
			case F.REASONING_END: return [...u(), e];
			case F.RAW:
			case F.ACTIVITY_SNAPSHOT:
			case F.ACTIVITY_DELTA:
			case F.REASONING_ENCRYPTED_VALUE: return [e];
			case F.TEXT_MESSAGE_CHUNK:
				let t = e, s = [];
				if ((o !== "text" || t.messageId !== void 0 && t.messageId !== r?.messageId) && s.push(...u()), o !== "text") {
					if (t.messageId === void 0) throw Error("First TEXT_MESSAGE_CHUNK must have a messageId");
					r = {
						messageId: t.messageId,
						name: t.name
					}, o = "text";
					let e = {
						type: F.TEXT_MESSAGE_START,
						messageId: t.messageId,
						role: t.role || "assistant",
						...t.name !== void 0 && { name: t.name }
					};
					s.push(e), n?.event("TRANSFORM", "TEXT_MESSAGE_START", e, { messageId: t.messageId });
				}
				if (t.delta !== void 0) {
					let e = {
						type: F.TEXT_MESSAGE_CONTENT,
						messageId: r.messageId,
						delta: t.delta
					};
					s.push(e), n?.event("TRANSFORM", "TEXT_MESSAGE_CONTENT", e, { messageId: r.messageId });
				}
				return s;
			case F.TOOL_CALL_CHUNK:
				let c = e, l = [];
				if ((o !== "tool" || c.toolCallId !== void 0 && c.toolCallId !== i?.toolCallId) && l.push(...u()), o !== "tool") {
					if (c.toolCallId === void 0) throw Error("First TOOL_CALL_CHUNK must have a toolCallId");
					if (c.toolCallName === void 0) throw Error("First TOOL_CALL_CHUNK must have a toolCallName");
					i = {
						toolCallId: c.toolCallId,
						toolCallName: c.toolCallName,
						parentMessageId: c.parentMessageId
					}, o = "tool";
					let e = {
						type: F.TOOL_CALL_START,
						toolCallId: c.toolCallId,
						toolCallName: c.toolCallName,
						parentMessageId: c.parentMessageId
					};
					l.push(e), n?.event("TRANSFORM", "TOOL_CALL_START", e, {
						toolCallId: c.toolCallId,
						toolCallName: c.toolCallName
					});
				}
				if (c.delta !== void 0) {
					let e = {
						type: F.TOOL_CALL_ARGS,
						toolCallId: i.toolCallId,
						delta: c.delta
					};
					l.push(e), n?.event("TRANSFORM", "TOOL_CALL_ARGS", e, { toolCallId: i.toolCallId });
				}
				return l;
			case F.REASONING_MESSAGE_CHUNK:
				let d = e, f = [];
				if ((o !== "reasoning" || d.messageId && d.messageId !== a?.messageId) && f.push(...u()), o !== "reasoning") {
					if (d.messageId === void 0) throw Error("First REASONING_MESSAGE_CHUNK must have a messageId");
					a = { messageId: d.messageId }, o = "reasoning";
					let e = {
						type: F.REASONING_MESSAGE_START,
						messageId: d.messageId
					};
					f.push(e), n?.event("TRANSFORM", "REASONING_MESSAGE_START", e, { messageId: d.messageId });
				}
				if (d.delta !== void 0) {
					let e = {
						type: F.REASONING_MESSAGE_CONTENT,
						messageId: a.messageId,
						delta: d.delta
					};
					f.push(e), n?.event("TRANSFORM", "REASONING_MESSAGE_CONTENT", e, { messageId: a.messageId });
				}
				return f;
		}
		return e.type, [];
	}), Zi(() => {
		u();
	}));
}, Ss = class {
	runNext(e, t) {
		return t.run(e).pipe(xs(!1));
	}
	runNextWithState(e, t) {
		let n = Z(e.messages || []), r = Z(e.state || {}), i = new si();
		return us(e, i, t, []).subscribe((e) => {
			e.messages !== void 0 && (n = e.messages), e.state !== void 0 && (r = e.state);
		}), this.runNext(e, t).pipe(Yi(async (e) => (i.next(e), await new Promise((e) => setTimeout(e, 0)), {
			event: e,
			messages: Z(n),
			state: Z(r)
		})));
	}
}, Cs = class extends Ss {
	constructor(e) {
		super(), this.fn = e;
	}
	run(e, t) {
		return this.fn(e, t);
	}
};
function ws(e) {
	let t = e.content;
	if (Array.isArray(t)) {
		let n = t.filter((e) => typeof e == "object" && !!e && "type" in e && e.type === "text" && typeof e.text == "string").map((e) => e.text).join("");
		return {
			...e,
			content: n
		};
	}
	return typeof t == "string" ? e : {
		...e,
		content: ""
	};
}
var Ts = class extends Ss {
	run(e, t) {
		let { parentRunId: n, ...r } = e, i = {
			...r,
			messages: r.messages.map(ws)
		};
		return this.runNext(i, t);
	}
}, Es = "THINKING_START", Ds = "THINKING_END", Os = "THINKING_TEXT_MESSAGE_START", ks = "THINKING_TEXT_MESSAGE_CONTENT", As = "THINKING_TEXT_MESSAGE_END", js = class extends Ss {
	constructor(...e) {
		super(...e), this.currentReasoningId = null, this.currentMessageId = null;
	}
	warnAboutTransformation(e, t) {
		typeof process < "u" && process.env !== void 0 && process.env.SUPPRESS_TRANSFORMATION_WARNINGS || console.warn(`AG-UI is converting ${e} to ${t}. To remove this warning, upgrade your AG-UI integration package (e.g. @ag-ui/langgraph). To surpress it, set SUPPRESS_TRANSFORMATION_WARNINGS=true in your .env file.`);
	}
	run(e, t) {
		return this.currentReasoningId = null, this.currentMessageId = null, this.runNext(e, t).pipe(Ui((e) => this.transformEvent(e)));
	}
	transformEvent(e) {
		switch (e.type) {
			case Es: {
				this.currentReasoningId = rs();
				let { title: t, ...n } = e;
				return this.warnAboutTransformation(Es, F.REASONING_START), {
					...n,
					type: F.REASONING_START,
					messageId: this.currentReasoningId
				};
			}
			case Os: return this.currentMessageId = rs(), this.warnAboutTransformation(Os, F.REASONING_MESSAGE_START), {
				...e,
				type: F.REASONING_MESSAGE_START,
				messageId: this.currentMessageId,
				role: "assistant"
			};
			case ks: {
				let { delta: t, ...n } = e;
				return this.warnAboutTransformation(ks, F.REASONING_MESSAGE_CONTENT), {
					...n,
					type: F.REASONING_MESSAGE_CONTENT,
					messageId: this.currentMessageId ?? rs(),
					delta: t
				};
			}
			case As: {
				let t = this.currentMessageId ?? rs();
				return this.warnAboutTransformation(As, F.REASONING_MESSAGE_END), {
					...e,
					type: F.REASONING_MESSAGE_END,
					messageId: t
				};
			}
			case Ds: {
				let t = this.currentReasoningId ?? rs();
				return this.warnAboutTransformation(Ds, F.REASONING_END), {
					...e,
					type: F.REASONING_END,
					messageId: t
				};
			}
			default: return e;
		}
	}
};
function Ms(e) {
	return e.startsWith("image/") ? "image" : e.startsWith("audio/") ? "audio" : e.startsWith("video/") ? "video" : "document";
}
function Ns(e) {
	return typeof e == "object" && !!e && "type" in e && e.type === "binary" && "mimeType" in e && typeof e.mimeType == "string";
}
function Ps(e) {
	let t = Ms(e.mimeType);
	return e.data ? {
		type: t,
		source: {
			type: "data",
			value: e.data,
			mimeType: e.mimeType
		},
		...e.filename ? { metadata: { filename: e.filename } } : {}
	} : e.url ? {
		type: t,
		source: {
			type: "url",
			value: e.url,
			mimeType: e.mimeType
		},
		...e.filename ? { metadata: { filename: e.filename } } : {}
	} : e;
}
function Fs(e) {
	let t = e.content;
	if (!Array.isArray(t)) return e;
	let n = t.map((e) => Ns(e) ? Ps(e) : e);
	return {
		...e,
		content: n
	};
}
var Is = class extends Ss {
	run(e, t) {
		let n = {
			...e,
			messages: e.messages.map(Fs)
		};
		return this.runNext(n, t);
	}
}, Ls = "0.0.53", Rs = class {
	get maxVersion() {
		return Ls;
	}
	get debug() {
		return this._debug;
	}
	set debug(e) {
		this._debug = as(e), this._debugLogger = cs(this._debug);
	}
	get debugLogger() {
		return this._debugLogger;
	}
	set debugLogger(e) {
		typeof e == "boolean" ? this._debugLogger = e ? cs(as(!0)) : void 0 : this._debugLogger = e;
	}
	constructor({ agentId: e, description: t, threadId: n, initialMessages: r, initialState: i, debug: a } = {}) {
		this.subscribers = [], this.isRunning = !1, this.middlewares = [], this.agentId = e, this.description = t ?? "", this.threadId = n ?? c(), this.messages = Z(r ?? []), this.state = Z(i ?? {}), this._debug = as(a), this._debugLogger = cs(this._debug), ns(this.maxVersion, "0.0.39") <= 0 && this.middlewares.unshift(new Ts()), ns(this.maxVersion, "0.0.45") <= 0 && this.middlewares.unshift(new js()), ns(this.maxVersion, "0.0.47") <= 0 && this.middlewares.unshift(new Is());
	}
	subscribe(e) {
		return this.subscribers.push(e), { unsubscribe: () => {
			this.subscribers = this.subscribers.filter((t) => t !== e);
		} };
	}
	use(...e) {
		let t = e.map((e) => typeof e == "function" ? new Cs(e) : e);
		return this.middlewares.push(...t), this;
	}
	async runAgent(e, t) {
		try {
			this.isRunning = !0, this.agentId = this.agentId ?? c();
			let n = this.prepareRunAgentInput(e);
			this.debugLogger?.lifecycle("LIFECYCLE", "Run started:", {
				agentId: this.agentId,
				threadId: this.threadId
			});
			let r, i = new Set(this.messages.map((e) => e.id)), a = [
				{ onRunFinishedEvent: (e) => {
					r = e.result;
				} },
				...this.subscribers,
				t ?? {}
			];
			await this.onInitialize(n, a), this.activeRunDetach$ = new ii();
			let o;
			this.activeRunCompletionPromise = new Promise((e) => {
				o = e;
			}), await Hi(Xr(() => this.middlewares.length === 0 ? this.run(n) : this.middlewares.reduceRight((e, t) => ({
				run: (n) => t.run(n, e),
				get messages() {
					return e.messages;
				},
				get state() {
					return e.state;
				}
			}), this).run(n), xs(this.debugLogger), ds(this.debugLogger), (e) => e.pipe($i(this.activeRunDetach$)), (e) => this.apply(n, e, a), (e) => this.processApplyEvents(n, e, a), Ji((e) => (this.debugLogger?.lifecycle("LIFECYCLE", "Run errored:", {
				agentId: this.agentId,
				error: e instanceof Error ? e.message : String(e)
			}), this.isRunning = !1, this.onError(n, e, a))), Zi(() => {
				this.debugLogger?.lifecycle("LIFECYCLE", "Run finished:", {
					agentId: this.agentId,
					threadId: this.threadId
				}), this.isRunning = !1, this.onFinalize(n, a), o?.(), o = void 0, this.activeRunCompletionPromise = void 0, this.activeRunDetach$ = void 0;
			}))(U(null)));
			let s = Z(this.messages).filter((e) => !i.has(e.id));
			return {
				result: r,
				newMessages: s
			};
		} finally {
			this.isRunning = !1;
		}
	}
	connect(e) {
		throw new Vt();
	}
	async connectAgent(e, t) {
		try {
			this.isRunning = !0, this.agentId = this.agentId ?? c();
			let n = this.prepareRunAgentInput(e), r, i = new Set(this.messages.map((e) => e.id)), a = [
				{ onRunFinishedEvent: (e) => {
					r = e.result;
				} },
				...this.subscribers,
				t ?? {}
			];
			await this.onInitialize(n, a), this.activeRunDetach$ = new ii();
			let o;
			this.activeRunCompletionPromise = new Promise((e) => {
				o = e;
			}), await Hi(Xr(() => qi(() => this.connect(n)), xs(this.debugLogger), ds(this.debugLogger), (e) => e.pipe($i(this.activeRunDetach$)), (e) => this.apply(n, e, a), (e) => this.processApplyEvents(n, e, a), Ji((e) => (this.isRunning = !1, e instanceof Vt ? ci : this.onError(n, e, a))), Zi(() => {
				this.isRunning = !1, this.onFinalize(n, a), o?.(), o = void 0, this.activeRunCompletionPromise = void 0, this.activeRunDetach$ = void 0;
			}))(U(null)), { defaultValue: void 0 });
			let s = Z(this.messages).filter((e) => !i.has(e.id));
			return {
				result: r,
				newMessages: s
			};
		} finally {
			this.isRunning = !1;
		}
	}
	abortRun() {}
	async detachActiveRun() {
		if (!this.activeRunDetach$) return;
		let e = this.activeRunCompletionPromise ?? Promise.resolve();
		this.activeRunDetach$.next(), this.activeRunDetach$?.complete(), await e;
	}
	apply(e, t, n) {
		return us(e, t, this, n, this.debugLogger);
	}
	processApplyEvents(e, t, n) {
		return t.pipe(ea((t) => {
			t.messages && (this.messages = t.messages, n.forEach((t) => {
				t.onMessagesChanged?.({
					messages: this.messages,
					state: this.state,
					agent: this,
					input: e
				});
			})), t.state && (this.state = t.state, n.forEach((t) => {
				t.onStateChanged?.({
					state: this.state,
					messages: this.messages,
					agent: this,
					input: e
				});
			}));
		}));
	}
	prepareRunAgentInput(e) {
		let t = Z(this.messages).filter((e) => e.role !== "activity");
		return {
			threadId: this.threadId,
			runId: e?.runId || c(),
			tools: Z(e?.tools ?? []),
			context: Z(e?.context ?? []),
			forwardedProps: Z(e?.forwardedProps ?? {}),
			state: Z(this.state),
			messages: t
		};
	}
	async onInitialize(e, t) {
		let n = await Q(t, this.messages, this.state, (t, n, r) => t.onRunInitialized?.({
			messages: n,
			state: r,
			agent: this,
			input: e
		}));
		(n.messages !== void 0 || n.state !== void 0) && (n.messages && (this.messages = n.messages, e.messages = n.messages, t.forEach((t) => {
			t.onMessagesChanged?.({
				messages: this.messages,
				state: this.state,
				agent: this,
				input: e
			});
		})), n.state && (this.state = n.state, e.state = n.state, t.forEach((t) => {
			t.onStateChanged?.({
				state: this.state,
				messages: this.messages,
				agent: this,
				input: e
			});
		})));
	}
	onError(e, t, n) {
		return Bi(Q(n, this.messages, this.state, (n, r, i) => n.onRunFailed?.({
			error: t,
			messages: r,
			state: i,
			agent: this,
			input: e
		}))).pipe(Ui((r) => {
			let i = r;
			if ((i.messages !== void 0 || i.state !== void 0) && (i.messages !== void 0 && (this.messages = i.messages, n.forEach((t) => {
				t.onMessagesChanged?.({
					messages: this.messages,
					state: this.state,
					agent: this,
					input: e
				});
			})), i.state !== void 0 && (this.state = i.state, n.forEach((t) => {
				t.onStateChanged?.({
					state: this.state,
					messages: this.messages,
					agent: this,
					input: e
				});
			}))), i.stopPropagation !== !0) {
				let e = String(t);
				if (!(t.name === "AbortError" || t.message === "Fetch is aborted" || t.message === "signal is aborted without reason" || t.message === "component unmounted" || e === "component unmounted")) throw console.error("Agent execution failed:", t), t;
			}
			return {};
		}));
	}
	async onFinalize(e, t) {
		let n = await Q(t, this.messages, this.state, (t, n, r) => t.onRunFinalized?.({
			messages: n,
			state: r,
			agent: this,
			input: e
		}));
		(n.messages !== void 0 || n.state !== void 0) && (n.messages !== void 0 && (this.messages = n.messages, t.forEach((t) => {
			t.onMessagesChanged?.({
				messages: this.messages,
				state: this.state,
				agent: this,
				input: e
			});
		})), n.state !== void 0 && (this.state = n.state, t.forEach((t) => {
			t.onStateChanged?.({
				state: this.state,
				messages: this.messages,
				agent: this,
				input: e
			});
		})));
	}
	clone() {
		let e = Object.create(Object.getPrototypeOf(this));
		return e.agentId = this.agentId, e.description = this.description, e.threadId = this.threadId, e.messages = Z(this.messages), e.state = Z(this.state), e._debug = this._debug, e._debugLogger = this._debugLogger, e.isRunning = this.isRunning, e.subscribers = [...this.subscribers], e.middlewares = [...this.middlewares], e;
	}
	addMessage(e) {
		this.messages.push(e), (async () => {
			for (let t of this.subscribers) await t.onNewMessage?.({
				message: e,
				messages: this.messages,
				state: this.state,
				agent: this
			});
			if (e.role === "assistant" && e.toolCalls) for (let t of e.toolCalls) for (let e of this.subscribers) await e.onNewToolCall?.({
				toolCall: t,
				messages: this.messages,
				state: this.state,
				agent: this
			});
			for (let e of this.subscribers) await e.onMessagesChanged?.({
				messages: this.messages,
				state: this.state,
				agent: this
			});
		})();
	}
	addMessages(e) {
		this.messages.push(...e), (async () => {
			for (let t of e) {
				for (let e of this.subscribers) await e.onNewMessage?.({
					message: t,
					messages: this.messages,
					state: this.state,
					agent: this
				});
				if (t.role === "assistant" && t.toolCalls) for (let e of t.toolCalls) for (let t of this.subscribers) await t.onNewToolCall?.({
					toolCall: e,
					messages: this.messages,
					state: this.state,
					agent: this
				});
			}
			for (let e of this.subscribers) await e.onMessagesChanged?.({
				messages: this.messages,
				state: this.state,
				agent: this
			});
		})();
	}
	setMessages(e) {
		this.messages = Z(e), (async () => {
			for (let e of this.subscribers) await e.onMessagesChanged?.({
				messages: this.messages,
				state: this.state,
				agent: this
			});
		})();
	}
	setState(e) {
		this.state = Z(e), (async () => {
			for (let e of this.subscribers) await e.onStateChanged?.({
				messages: this.messages,
				state: this.state,
				agent: this
			});
		})();
	}
	legacy_to_be_removed_runAgentBridged(e) {
		this.agentId = this.agentId ?? c();
		let t = this.prepareRunAgentInput(e);
		return (this.middlewares.length === 0 ? this.run(t) : this.middlewares.reduceRight((e, t) => ({
			run: (n) => t.run(n, e),
			get messages() {
				return e.messages;
			},
			get state() {
				return e.state;
			}
		}), this).run(t)).pipe(xs(this.debugLogger), ds(this.debugLogger), ys(this.threadId, t.runId, this.agentId), (e) => e.pipe(Ui((e) => (this.debugLogger?.event("LEGACY", "Event:", e, { type: e.type }), e))));
	}
}, zs = class extends Rs {
	requestInit(e) {
		return {
			method: "POST",
			headers: {
				...this.headers,
				"Content-Type": "application/json",
				Accept: "text/event-stream"
			},
			body: JSON.stringify(e),
			signal: this.abortController.signal
		};
	}
	runAgent(e, t) {
		return this.abortController = e?.abortController ?? new AbortController(), super.runAgent(e, t);
	}
	abortRun() {
		this.abortController.abort(), super.abortRun();
	}
	constructor(e) {
		super(e), this.abortController = new AbortController(), this.url = e.url, this.headers = Z(e.headers ?? {});
	}
	run(e) {
		return gs(ps(this.url, this.requestInit(e)), this.debugLogger);
	}
	clone() {
		let e = super.clone();
		e.url = this.url, e.headers = Z(this.headers ?? {});
		let t = new AbortController(), n = this.abortController.signal;
		return n.aborted && t.abort(n.reason), e.abortController = t, e;
	}
}, Bs = document.getElementById("messages"), Vs = document.getElementById("input"), Hs = document.getElementById("send"), Us = localStorage.getItem("ag-ui-thread-id") ?? crypto.randomUUID();
localStorage.setItem("ag-ui-thread-id", Us);
var Ws = [];
function Gs(e, t) {
	let n = document.createElement("div");
	return n.className = `msg ${e}`, n.textContent = t, Bs.appendChild(n), Bs.scrollTop = Bs.scrollHeight, n;
}
function Ks() {
	for (let e of Ws) e.role === "user" && typeof e.content == "string" ? Gs("user", e.content) : e.role === "assistant" && typeof e.content == "string" && e.content.length > 0 && Gs("assistant", e.content);
}
async function qs() {
	let e = await fetch(`/history/${encodeURIComponent(Us)}`);
	if (!e.ok) return;
	let t = await e.json(), n = /* @__PURE__ */ new Set();
	for (let e of t) {
		let t = e.messageId ?? crypto.randomUUID();
		if (n.has(t)) continue;
		n.add(t);
		let r = e.contents.find((e) => e.$type === "text")?.text;
		r && (e.role === "user" ? Ws.push({
			id: t,
			role: "user",
			content: r
		}) : e.role === "assistant" && Ws.push({
			id: t,
			role: "assistant",
			content: r
		}));
	}
	Ws.length > 0 && Ks();
}
var Js = /* @__PURE__ */ new Map();
function Ys(e) {
	let t = document.createElement("div");
	t.className = "msg approval-request", t.dataset.toolCallId = e.toolCallId, t.innerHTML = `
		<div class="approval-header"><span>&#9888;</span> Approval Required</div>
		<div class="approval-tool">Tool: <code>${e.functionName}</code></div>
		<div class="approval-actions">
			<button class="approve-btn">Approve</button>
			<button class="deny-btn">Deny</button>
		</div>`, t.querySelector(".approve-btn").addEventListener("click", () => {
		t.remove(), Js.delete(e.toolCallId), e.resolve(!0);
	}), t.querySelector(".deny-btn").addEventListener("click", () => {
		t.remove(), Js.delete(e.toolCallId), e.resolve(!1);
	}), Bs.appendChild(t), Bs.scrollTop = Bs.scrollHeight;
}
async function Xs(e, t) {
	let n = new zs({ url: "/agui" }), r = t.textContent === "…" ? "" : t.textContent ?? "", i = /* @__PURE__ */ new Map(), a = /* @__PURE__ */ new Map(), o = [];
	await new Promise((s, c) => {
		n.run(e).subscribe({
			next: (n) => {
				switch (n.type) {
					case F.TEXT_MESSAGE_CONTENT:
						r += n.delta ?? "", t.textContent = r, Bs.scrollTop = Bs.scrollHeight;
						break;
					case F.TOOL_CALL_START: {
						let e = n;
						a.set(e.toolCallId, e.toolCallName), i.set(e.toolCallId, "");
						break;
					}
					case F.TOOL_CALL_ARGS: {
						let e = n;
						i.set(e.toolCallId, (i.get(e.toolCallId) ?? "") + e.delta);
						break;
					}
					case F.TOOL_CALL_END: {
						let r = n.toolCallId, s = a.get(r) ?? "", c = i.get(r) ?? "{}";
						if (s !== "request_approval") break;
						o.push({
							id: r,
							name: s,
							args: c
						});
						let l = r, u = "unknown";
						try {
							let e = JSON.parse(c);
							if (e.request) {
								let t = JSON.parse(e.request);
								l = t.approval_id ?? r, u = t.function_name ?? "unknown";
							}
						} catch {}
						let d = {
							toolCallId: r,
							approvalId: l,
							functionName: u,
							resolve: async (n) => {
								let i = {
									id: crypto.randomUUID(),
									role: "assistant",
									toolCalls: o.map((e) => ({
										id: e.id,
										type: "function",
										function: {
											name: e.name,
											arguments: e.args
										}
									}))
								}, a = {
									id: crypto.randomUUID(),
									role: "tool",
									toolCallId: r,
									content: JSON.stringify({
										approval_id: l,
										approved: n
									})
								};
								await Xs({
									...e,
									runId: crypto.randomUUID(),
									messages: [
										...e.messages,
										i,
										a
									]
								}, t);
							}
						};
						Js.set(r, d), Ys(d);
						break;
					}
				}
			},
			error: (e) => {
				t.textContent = `Error: ${e}`, Hs.disabled = !1, c(e);
			},
			complete: () => s()
		});
	});
}
async function Zs() {
	let e = Vs.value.trim();
	if (!e) return;
	Vs.value = "", Hs.disabled = !0;
	let t = {
		id: crypto.randomUUID(),
		role: "user",
		content: e
	};
	Ws.push(t), Gs("user", e);
	let n = Gs("assistant", "…"), r = {
		threadId: Us,
		runId: crypto.randomUUID(),
		messages: [t],
		tools: [],
		context: []
	};
	try {
		await Xs(r, n), n.textContent && n.textContent !== "…" && Ws.push({
			id: crypto.randomUUID(),
			role: "assistant",
			content: n.textContent
		});
	} finally {
		Hs.disabled = !1, Vs.focus();
	}
}
Hs.addEventListener("click", Zs), Vs.addEventListener("keydown", (e) => {
	e.key === "Enter" && !e.shiftKey && Zs();
}), qs(), Vs.focus();
//#endregion
