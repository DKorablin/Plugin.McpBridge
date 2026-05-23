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
		return new ut({
			schema: this,
			typeName: T.ZodEffects,
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
		return dt.create(this, this._def);
	}
	nullable() {
		return ft.create(this, this._def);
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
		return new ut({
			...S(this._def),
			schema: this,
			typeName: T.ZodEffects,
			effect: {
				type: "transform",
				transform: e
			}
		});
	}
	default(e) {
		let t = typeof e == "function" ? e : () => e;
		return new pt({
			...S(this._def),
			innerType: this,
			defaultValue: t,
			typeName: T.ZodDefault
		});
	}
	brand() {
		return new gt({
			typeName: T.ZodBranded,
			type: this,
			...S(this._def)
		});
	}
	catch(e) {
		let t = typeof e == "function" ? e : () => e;
		return new mt({
			...S(this._def),
			innerType: this,
			catchValue: t,
			typeName: T.ZodCatch
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
		return _t.create(this, e);
	}
	readonly() {
		return vt.create(this);
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
	typeName: T.ZodString,
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
	typeName: T.ZodNumber,
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
	typeName: T.ZodBigInt,
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
	typeName: T.ZodBoolean,
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
	typeName: T.ZodDate,
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
	typeName: T.ZodSymbol,
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
	typeName: T.ZodUndefined,
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
	typeName: T.ZodNull,
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
	typeName: T.ZodAny,
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
	typeName: T.ZodUnknown,
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
	typeName: T.ZodNever,
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
	typeName: T.ZodVoid,
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
	typeName: T.ZodArray,
	...S(t)
});
function qe(e) {
	if (e instanceof w) {
		let t = {};
		for (let n in e.shape) {
			let r = e.shape[n];
			t[n] = dt.create(qe(r));
		}
		return new w({
			...e._def,
			shape: () => t
		});
	} else if (e instanceof Ke) return new Ke({
		...e._def,
		type: qe(e.element)
	});
	else if (e instanceof dt) return dt.create(qe(e.unwrap()));
	else if (e instanceof ft) return ft.create(qe(e.unwrap()));
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
			typeName: T.ZodObject
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
			for (; t instanceof dt;) t = t._def.innerType;
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
	typeName: T.ZodObject,
	...S(t)
}), w.strictCreate = (e, t) => new w({
	shape: () => e,
	unknownKeys: "strict",
	catchall: We.create(),
	typeName: T.ZodObject,
	...S(t)
}), w.lazycreate = (e, t) => new w({
	shape: e,
	unknownKeys: "strip",
	catchall: We.create(),
	typeName: T.ZodObject,
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
	typeName: T.ZodUnion,
	...S(t)
});
var Ye = (e) => e instanceof it ? Ye(e.schema) : e instanceof ut ? Ye(e.innerType()) : e instanceof at ? [e.value] : e instanceof st ? e.options : e instanceof ct ? l.objectValues(e.enum) : e instanceof pt ? Ye(e._def.innerType) : e instanceof Be ? [void 0] : e instanceof Ve ? [null] : e instanceof dt ? [void 0, ...Ye(e.unwrap())] : e instanceof ft ? [null, ...Ye(e.unwrap())] : e instanceof gt || e instanceof vt ? Ye(e.unwrap()) : e instanceof mt ? Ye(e._def.innerType) : [], Xe = class e extends C {
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
			typeName: T.ZodDiscriminatedUnion,
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
	typeName: T.ZodIntersection,
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
		typeName: T.ZodTuple,
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
			typeName: T.ZodRecord,
			...S(r)
		}) : new e({
			keyType: Ne.create(),
			valueType: t,
			typeName: T.ZodRecord,
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
	typeName: T.ZodMap,
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
	typeName: T.ZodSet,
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
			typeName: T.ZodFunction,
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
	typeName: T.ZodLazy,
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
	typeName: T.ZodLiteral,
	...S(t)
});
function ot(e, t) {
	return new st({
		values: e,
		typeName: T.ZodEnum,
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
	typeName: T.ZodNativeEnum,
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
	typeName: T.ZodPromise,
	...S(t)
});
var ut = class extends C {
	innerType() {
		return this._def.schema;
	}
	sourceType() {
		return this._def.schema._def.typeName === T.ZodEffects ? this._def.schema.sourceType() : this._def.schema;
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
ut.create = (e, t, n) => new ut({
	schema: e,
	typeName: T.ZodEffects,
	effect: t,
	...S(n)
}), ut.createWithPreprocess = (e, t, n) => new ut({
	schema: t,
	effect: {
		type: "preprocess",
		transform: e
	},
	typeName: T.ZodEffects,
	...S(n)
});
var dt = class extends C {
	_parse(e) {
		return this._getType(e) === d.undefined ? y(void 0) : this._def.innerType._parse(e);
	}
	unwrap() {
		return this._def.innerType;
	}
};
dt.create = (e, t) => new dt({
	innerType: e,
	typeName: T.ZodOptional,
	...S(t)
});
var ft = class extends C {
	_parse(e) {
		return this._getType(e) === d.null ? y(null) : this._def.innerType._parse(e);
	}
	unwrap() {
		return this._def.innerType;
	}
};
ft.create = (e, t) => new ft({
	innerType: e,
	typeName: T.ZodNullable,
	...S(t)
});
var pt = class extends C {
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
pt.create = (e, t) => new pt({
	innerType: e,
	typeName: T.ZodDefault,
	defaultValue: typeof t.default == "function" ? t.default : () => t.default,
	...S(t)
});
var mt = class extends C {
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
mt.create = (e, t) => new mt({
	innerType: e,
	typeName: T.ZodCatch,
	catchValue: typeof t.catch == "function" ? t.catch : () => t.catch,
	...S(t)
});
var ht = class extends C {
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
ht.create = (e) => new ht({
	typeName: T.ZodNaN,
	...S(e)
});
var gt = class extends C {
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
}, _t = class e extends C {
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
			typeName: T.ZodPipeline
		});
	}
}, vt = class extends C {
	_parse(e) {
		let t = this._def.innerType._parse(e), n = (e) => (oe(e) && (e.value = Object.freeze(e.value)), e);
		return se(t) ? t.then((e) => n(e)) : n(t);
	}
	unwrap() {
		return this._def.innerType;
	}
};
vt.create = (e, t) => new vt({
	innerType: e,
	typeName: T.ZodReadonly,
	...S(t)
}), w.lazycreate;
var T;
(function(e) {
	e.ZodString = "ZodString", e.ZodNumber = "ZodNumber", e.ZodNaN = "ZodNaN", e.ZodBigInt = "ZodBigInt", e.ZodBoolean = "ZodBoolean", e.ZodDate = "ZodDate", e.ZodSymbol = "ZodSymbol", e.ZodUndefined = "ZodUndefined", e.ZodNull = "ZodNull", e.ZodAny = "ZodAny", e.ZodUnknown = "ZodUnknown", e.ZodNever = "ZodNever", e.ZodVoid = "ZodVoid", e.ZodArray = "ZodArray", e.ZodObject = "ZodObject", e.ZodUnion = "ZodUnion", e.ZodDiscriminatedUnion = "ZodDiscriminatedUnion", e.ZodIntersection = "ZodIntersection", e.ZodTuple = "ZodTuple", e.ZodRecord = "ZodRecord", e.ZodMap = "ZodMap", e.ZodSet = "ZodSet", e.ZodFunction = "ZodFunction", e.ZodLazy = "ZodLazy", e.ZodLiteral = "ZodLiteral", e.ZodEnum = "ZodEnum", e.ZodEffects = "ZodEffects", e.ZodNativeEnum = "ZodNativeEnum", e.ZodOptional = "ZodOptional", e.ZodNullable = "ZodNullable", e.ZodDefault = "ZodDefault", e.ZodCatch = "ZodCatch", e.ZodPromise = "ZodPromise", e.ZodBranded = "ZodBranded", e.ZodPipeline = "ZodPipeline", e.ZodReadonly = "ZodReadonly";
})(T ||= {});
var E = Ne.create, yt = Fe.create;
ht.create, Ie.create;
var D = Le.create;
Re.create, ze.create, Be.create, Ve.create;
var O = He.create, bt = Ue.create;
We.create, Ge.create;
var k = Ke.create, A = w.create;
w.strictCreate;
var xt = Je.create, St = Xe.create;
Qe.create, $e.create;
var Ct = et.create;
tt.create, nt.create, rt.create, it.create;
var j = at.create, wt = st.create, Tt = ct.create;
lt.create, ut.create, dt.create, ft.create, ut.createWithPreprocess, _t.create;
//#endregion
//#region node_modules/@ag-ui/core/dist/index.mjs
var Et = A({
	name: E(),
	arguments: E()
}), Dt = A({
	id: E(),
	type: j("function"),
	function: Et,
	encryptedValue: E().optional()
}), Ot = A({
	id: E(),
	role: E(),
	content: E().optional(),
	name: E().optional(),
	encryptedValue: E().optional()
}), kt = A({
	type: j("text"),
	text: E()
}), At = St("type", [A({
	type: j("data"),
	value: E(),
	mimeType: E()
}), A({
	type: j("url"),
	value: E(),
	mimeType: E().optional()
})]), jt = A({
	type: j("image"),
	source: At,
	metadata: bt().optional()
}), Mt = A({
	type: j("audio"),
	source: At,
	metadata: bt().optional()
}), Nt = A({
	type: j("video"),
	source: At,
	metadata: bt().optional()
}), Pt = A({
	type: j("document"),
	source: At,
	metadata: bt().optional()
}), Ft = A({
	type: j("binary"),
	mimeType: E(),
	id: E().optional(),
	url: E().optional(),
	data: E().optional(),
	filename: E().optional()
}), It = (e, t) => {
	!e.id && !e.url && !e.data && t.addIssue({
		code: p.custom,
		message: "BinaryInputContent requires at least one of id, url, or data.",
		path: ["id"]
	});
};
Ft.superRefine((e, t) => {
	It(e, t);
});
var Lt = St("type", [
	kt,
	jt,
	Mt,
	Nt,
	Pt,
	Ft
]).superRefine((e, t) => {
	e.type === "binary" && It(e, t);
}), Rt = St("role", [
	Ot.extend({
		role: j("developer"),
		content: E()
	}),
	Ot.extend({
		role: j("system"),
		content: E()
	}),
	Ot.extend({
		role: j("assistant"),
		content: E().optional(),
		toolCalls: k(Dt).optional()
	}),
	Ot.extend({
		role: j("user"),
		content: xt([E(), k(Lt)])
	}),
	A({
		id: E(),
		content: E(),
		role: j("tool"),
		toolCallId: E(),
		error: E().optional(),
		encryptedValue: E().optional()
	}),
	A({
		id: E(),
		role: j("activity"),
		activityType: E(),
		content: Ct(O())
	}),
	A({
		id: E(),
		role: j("reasoning"),
		content: E(),
		encryptedValue: E().optional()
	})
]);
xt([
	j("developer"),
	j("system"),
	j("assistant"),
	j("user"),
	j("tool"),
	j("activity"),
	j("reasoning")
]);
var zt = A({
	description: E(),
	value: E()
}), Bt = A({
	name: E(),
	description: E(),
	parameters: O(),
	metadata: Ct(O()).optional()
}), Vt = A({
	threadId: E(),
	runId: E(),
	parentRunId: E().optional(),
	state: O(),
	messages: k(Rt),
	tools: k(Bt),
	context: k(zt),
	forwardedProps: O()
}), Ht = O(), M = class extends Error {
	constructor(e) {
		super(e);
	}
}, Ut = class extends M {
	constructor() {
		super("Connect not implemented. This method is not supported by the current agent.");
	}
}, Wt = A({
	name: E(),
	description: E().optional()
}), Gt = A({
	name: E().optional(),
	type: E().optional(),
	description: E().optional(),
	version: E().optional(),
	provider: E().optional(),
	documentationUrl: E().optional(),
	metadata: Ct(bt()).optional()
}), Kt = A({
	streaming: D().optional(),
	websocket: D().optional(),
	httpBinary: D().optional(),
	pushNotifications: D().optional(),
	resumable: D().optional()
}), qt = A({
	supported: D().optional(),
	items: k(Bt).optional(),
	parallelCalls: D().optional(),
	clientProvided: D().optional()
}), Jt = A({
	structuredOutput: D().optional(),
	supportedMimeTypes: k(E()).optional()
}), Yt = A({
	snapshots: D().optional(),
	deltas: D().optional(),
	memory: D().optional(),
	persistentState: D().optional()
}), Xt = A({
	supported: D().optional(),
	delegation: D().optional(),
	handoffs: D().optional(),
	subAgents: k(Wt).optional()
}), Zt = A({
	supported: D().optional(),
	streaming: D().optional(),
	encrypted: D().optional()
}), Qt = A({
	image: D().optional(),
	audio: D().optional(),
	video: D().optional(),
	pdf: D().optional(),
	file: D().optional()
}), $t = A({
	image: D().optional(),
	audio: D().optional()
}), en = A({
	input: Qt.optional(),
	output: $t.optional()
}), tn = A({
	codeExecution: D().optional(),
	sandboxed: D().optional(),
	maxIterations: yt().optional(),
	maxExecutionTime: yt().optional()
}), nn = A({
	supported: D().optional(),
	approvals: D().optional(),
	interventions: D().optional(),
	feedback: D().optional()
});
A({
	identity: Gt.optional(),
	transport: Kt.optional(),
	tools: qt.optional(),
	output: Jt.optional(),
	state: Yt.optional(),
	multiAgent: Xt.optional(),
	reasoning: Zt.optional(),
	multimodal: en.optional(),
	execution: tn.optional(),
	humanInTheLoop: nn.optional(),
	custom: Ct(bt()).optional()
});
var rn = xt([
	j("developer"),
	j("system"),
	j("assistant"),
	j("user")
]), N = /* @__PURE__ */ function(e) {
	return e.TEXT_MESSAGE_START = "TEXT_MESSAGE_START", e.TEXT_MESSAGE_CONTENT = "TEXT_MESSAGE_CONTENT", e.TEXT_MESSAGE_END = "TEXT_MESSAGE_END", e.TEXT_MESSAGE_CHUNK = "TEXT_MESSAGE_CHUNK", e.TOOL_CALL_START = "TOOL_CALL_START", e.TOOL_CALL_ARGS = "TOOL_CALL_ARGS", e.TOOL_CALL_END = "TOOL_CALL_END", e.TOOL_CALL_CHUNK = "TOOL_CALL_CHUNK", e.TOOL_CALL_RESULT = "TOOL_CALL_RESULT", e.THINKING_START = "THINKING_START", e.THINKING_END = "THINKING_END", e.THINKING_TEXT_MESSAGE_START = "THINKING_TEXT_MESSAGE_START", e.THINKING_TEXT_MESSAGE_CONTENT = "THINKING_TEXT_MESSAGE_CONTENT", e.THINKING_TEXT_MESSAGE_END = "THINKING_TEXT_MESSAGE_END", e.STATE_SNAPSHOT = "STATE_SNAPSHOT", e.STATE_DELTA = "STATE_DELTA", e.MESSAGES_SNAPSHOT = "MESSAGES_SNAPSHOT", e.ACTIVITY_SNAPSHOT = "ACTIVITY_SNAPSHOT", e.ACTIVITY_DELTA = "ACTIVITY_DELTA", e.RAW = "RAW", e.CUSTOM = "CUSTOM", e.RUN_STARTED = "RUN_STARTED", e.RUN_FINISHED = "RUN_FINISHED", e.RUN_ERROR = "RUN_ERROR", e.STEP_STARTED = "STEP_STARTED", e.STEP_FINISHED = "STEP_FINISHED", e.REASONING_START = "REASONING_START", e.REASONING_MESSAGE_START = "REASONING_MESSAGE_START", e.REASONING_MESSAGE_CONTENT = "REASONING_MESSAGE_CONTENT", e.REASONING_MESSAGE_END = "REASONING_MESSAGE_END", e.REASONING_MESSAGE_CHUNK = "REASONING_MESSAGE_CHUNK", e.REASONING_END = "REASONING_END", e.REASONING_ENCRYPTED_VALUE = "REASONING_ENCRYPTED_VALUE", e;
}({}), P = A({
	type: Tt(N),
	timestamp: yt().optional(),
	rawEvent: O().optional()
}).passthrough(), an = P.extend({
	type: j(N.TEXT_MESSAGE_START),
	messageId: E(),
	role: rn.default("assistant"),
	name: E().optional()
}), on = P.extend({
	type: j(N.TEXT_MESSAGE_CONTENT),
	messageId: E(),
	delta: E()
}), sn = P.extend({
	type: j(N.TEXT_MESSAGE_END),
	messageId: E()
}), cn = P.extend({
	type: j(N.TEXT_MESSAGE_CHUNK),
	messageId: E().optional(),
	role: rn.optional(),
	delta: E().optional(),
	name: E().optional()
}), ln = P.extend({ type: j(N.THINKING_TEXT_MESSAGE_START) }), un = on.omit({
	messageId: !0,
	type: !0
}).extend({ type: j(N.THINKING_TEXT_MESSAGE_CONTENT) }), dn = P.extend({ type: j(N.THINKING_TEXT_MESSAGE_END) }), fn = P.extend({
	type: j(N.TOOL_CALL_START),
	toolCallId: E(),
	toolCallName: E(),
	parentMessageId: E().optional()
}), pn = P.extend({
	type: j(N.TOOL_CALL_ARGS),
	toolCallId: E(),
	delta: E()
}), mn = P.extend({
	type: j(N.TOOL_CALL_END),
	toolCallId: E()
}), hn = P.extend({
	messageId: E(),
	type: j(N.TOOL_CALL_RESULT),
	toolCallId: E(),
	content: E(),
	role: j("tool").optional()
}), gn = P.extend({
	type: j(N.TOOL_CALL_CHUNK),
	toolCallId: E().optional(),
	toolCallName: E().optional(),
	parentMessageId: E().optional(),
	delta: E().optional()
}), _n = P.extend({
	type: j(N.THINKING_START),
	title: E().optional()
}), vn = P.extend({ type: j(N.THINKING_END) }), yn = P.extend({
	type: j(N.STATE_SNAPSHOT),
	snapshot: Ht
}), bn = P.extend({
	type: j(N.STATE_DELTA),
	delta: k(O())
}), xn = P.extend({
	type: j(N.MESSAGES_SNAPSHOT),
	messages: k(Rt)
}), Sn = P.extend({
	type: j(N.ACTIVITY_SNAPSHOT),
	messageId: E(),
	activityType: E(),
	content: Ct(O()),
	replace: D().optional().default(!0)
}), Cn = P.extend({
	type: j(N.ACTIVITY_DELTA),
	messageId: E(),
	activityType: E(),
	patch: k(O())
}), wn = P.extend({
	type: j(N.RAW),
	event: O(),
	source: E().optional()
}), Tn = P.extend({
	type: j(N.CUSTOM),
	name: E(),
	value: O()
}), En = P.extend({
	type: j(N.RUN_STARTED),
	threadId: E(),
	runId: E(),
	parentRunId: E().optional(),
	input: Vt.optional()
}), Dn = P.extend({
	type: j(N.RUN_FINISHED),
	threadId: E(),
	runId: E(),
	result: O().optional()
}), On = P.extend({
	type: j(N.RUN_ERROR),
	message: E(),
	code: E().optional()
}), kn = P.extend({
	type: j(N.STEP_STARTED),
	stepName: E()
}), An = P.extend({
	type: j(N.STEP_FINISHED),
	stepName: E()
}), jn = xt([j("tool-call"), j("message")]), Mn = St("type", [
	an,
	on,
	sn,
	cn,
	_n,
	vn,
	ln,
	un,
	dn,
	fn,
	pn,
	mn,
	gn,
	hn,
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
	kn,
	An,
	P.extend({
		type: j(N.REASONING_START),
		messageId: E()
	}),
	P.extend({
		type: j(N.REASONING_MESSAGE_START),
		messageId: E(),
		role: j("reasoning")
	}),
	P.extend({
		type: j(N.REASONING_MESSAGE_CONTENT),
		messageId: E(),
		delta: E()
	}),
	P.extend({
		type: j(N.REASONING_MESSAGE_END),
		messageId: E()
	}),
	P.extend({
		type: j(N.REASONING_MESSAGE_CHUNK),
		messageId: E().optional(),
		delta: E().optional()
	}),
	P.extend({
		type: j(N.REASONING_END),
		messageId: E()
	}),
	P.extend({
		type: j(N.REASONING_ENCRYPTED_VALUE),
		subtype: jn,
		entityId: E(),
		encryptedValue: E()
	})
]), Nn = (function() {
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
})(), Pn = Object.prototype.hasOwnProperty;
function Fn(e, t) {
	return Pn.call(e, t);
}
function In(e) {
	if (Array.isArray(e)) {
		for (var t = Array(e.length), n = 0; n < t.length; n++) t[n] = "" + n;
		return t;
	}
	if (Object.keys) return Object.keys(e);
	var r = [];
	for (var i in e) Fn(e, i) && r.push(i);
	return r;
}
function F(e) {
	switch (typeof e) {
		case "object": return JSON.parse(JSON.stringify(e));
		case "undefined": return null;
		default: return e;
	}
}
function Ln(e) {
	for (var t = 0, n = e.length, r; t < n;) {
		if (r = e.charCodeAt(t), r >= 48 && r <= 57) {
			t++;
			continue;
		}
		return !1;
	}
	return !0;
}
function Rn(e) {
	return e.indexOf("/") === -1 && e.indexOf("~") === -1 ? e : e.replace(/~/g, "~0").replace(/\//g, "~1");
}
function zn(e) {
	return e.replace(/~1/g, "/").replace(/~0/g, "~");
}
function Bn(e) {
	if (e === void 0) return !0;
	if (e) {
		if (Array.isArray(e)) {
			for (var t = 0, n = e.length; t < n; t++) if (Bn(e[t])) return !0;
		} else if (typeof e == "object") {
			for (var r = In(e), i = r.length, a = 0; a < i; a++) if (Bn(e[r[a]])) return !0;
		}
	}
	return !1;
}
function Vn(e, t) {
	var n = [e];
	for (var r in t) {
		var i = typeof t[r] == "object" ? JSON.stringify(t[r], null, 2) : t[r];
		i !== void 0 && n.push(r + ": " + i);
	}
	return n.join("\n");
}
var Hn = function(e) {
	Nn(t, e);
	function t(t, n, r, i, a) {
		var o = this.constructor, s = e.call(this, Vn(t, {
			name: n,
			index: r,
			operation: i,
			tree: a
		})) || this;
		return s.name = n, s.index = r, s.operation = i, s.tree = a, Object.setPrototypeOf(s, o.prototype), s.message = Vn(t, {
			name: n,
			index: r,
			operation: i,
			tree: a
		}), s;
	}
	return t;
}(Error), Un = /* @__PURE__ */ t({
	JsonPatchError: () => I,
	_areEquals: () => $n,
	applyOperation: () => Jn,
	applyPatch: () => Yn,
	applyReducer: () => Xn,
	deepClone: () => Wn,
	getValueByPointer: () => qn,
	validate: () => Qn,
	validator: () => Zn
}), I = Hn, Wn = F, Gn = {
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
		var r = qn(n, this.path);
		r &&= F(r);
		var i = Jn(n, {
			op: "remove",
			path: this.from
		}).removed;
		return Jn(n, {
			op: "add",
			path: this.path,
			value: i
		}), {
			newDocument: n,
			removed: r
		};
	},
	copy: function(e, t, n) {
		var r = qn(n, this.from);
		return Jn(n, {
			op: "add",
			path: this.path,
			value: F(r)
		}), { newDocument: n };
	},
	test: function(e, t, n) {
		return {
			newDocument: n,
			test: $n(e[t], this.value)
		};
	},
	_get: function(e, t, n) {
		return this.value = e[t], { newDocument: n };
	}
}, Kn = {
	add: function(e, t, n) {
		return Ln(t) ? e.splice(t, 0, this.value) : e[t] = this.value, {
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
	move: Gn.move,
	copy: Gn.copy,
	test: Gn.test,
	_get: Gn._get
};
function qn(e, t) {
	if (t == "") return e;
	var n = {
		op: "_get",
		path: t
	};
	return Jn(e, n), n.value;
}
function Jn(e, t, n, r, i, a) {
	if (n === void 0 && (n = !1), r === void 0 && (r = !0), i === void 0 && (i = !0), a === void 0 && (a = 0), n && (typeof n == "function" ? n(t, 0, e, t.path) : Zn(t, 0)), t.path === "") {
		var o = { newDocument: e };
		if (t.op === "add") return o.newDocument = t.value, o;
		if (t.op === "replace") return o.newDocument = t.value, o.removed = e, o;
		if (t.op === "move" || t.op === "copy") return o.newDocument = qn(e, t.from), t.op === "move" && (o.removed = e), o;
		if (t.op === "test") {
			if (o.test = $n(e, t.value), o.test === !1) throw new I("Test operation failed", "TEST_OPERATION_FAILED", a, t, e);
			return o.newDocument = e, o;
		} else if (t.op === "remove") return o.removed = e, o.newDocument = null, o;
		else if (t.op === "_get") return t.value = e, o;
		else if (n) throw new I("Operation `op` property is not one of operations defined in RFC-6902", "OPERATION_OP_INVALID", a, t, e);
		else return o;
	} else {
		r || (e = F(e));
		var s = (t.path || "").split("/"), c = e, l = 1, u = s.length, d = void 0, f = void 0, p = void 0;
		for (p = typeof n == "function" ? n : Zn;;) {
			if (f = s[l], f && f.indexOf("~") != -1 && (f = zn(f)), i && (f == "__proto__" || f == "prototype" && l > 0 && s[l - 1] == "constructor")) throw TypeError("JSON-Patch: modifying `__proto__` or `constructor/prototype` prop is banned for security reasons, if this was on purpose, please set `banPrototypeModifications` flag false and pass it to this function. More info in fast-json-patch README");
			if (n && d === void 0 && (c[f] === void 0 ? d = s.slice(0, l).join("/") : l == u - 1 && (d = t.path), d !== void 0 && p(t, 0, e, d)), l++, Array.isArray(c)) {
				if (f === "-") f = c.length;
				else if (n && !Ln(f)) throw new I("Expected an unsigned base-10 integer value, making the new referenced value the array element with the zero-based index", "OPERATION_PATH_ILLEGAL_ARRAY_INDEX", a, t, e);
				else Ln(f) && (f = ~~f);
				if (l >= u) {
					if (n && t.op === "add" && f > c.length) throw new I("The specified index MUST NOT be greater than the number of elements in the array", "OPERATION_VALUE_OUT_OF_BOUNDS", a, t, e);
					var o = Kn[t.op].call(t, c, f, e);
					if (o.test === !1) throw new I("Test operation failed", "TEST_OPERATION_FAILED", a, t, e);
					return o;
				}
			} else if (l >= u) {
				var o = Gn[t.op].call(t, c, f, e);
				if (o.test === !1) throw new I("Test operation failed", "TEST_OPERATION_FAILED", a, t, e);
				return o;
			}
			if (c = c[f], n && l < u && (!c || typeof c != "object")) throw new I("Cannot perform operation at the desired path", "OPERATION_PATH_UNRESOLVABLE", a, t, e);
		}
	}
}
function Yn(e, t, n, r, i) {
	if (r === void 0 && (r = !0), i === void 0 && (i = !0), n && !Array.isArray(t)) throw new I("Patch sequence must be an array", "SEQUENCE_NOT_AN_ARRAY");
	r || (e = F(e));
	for (var a = Array(t.length), o = 0, s = t.length; o < s; o++) a[o] = Jn(e, t[o], n, !0, i, o), e = a[o].newDocument;
	return a.newDocument = e, a;
}
function Xn(e, t, n) {
	var r = Jn(e, t);
	if (r.test === !1) throw new I("Test operation failed", "TEST_OPERATION_FAILED", n, t, e);
	return r.newDocument;
}
function Zn(e, t, n, r) {
	if (typeof e != "object" || !e || Array.isArray(e)) throw new I("Operation is not an object", "OPERATION_NOT_AN_OBJECT", t, e, n);
	if (!Gn[e.op]) throw new I("Operation `op` property is not one of operations defined in RFC-6902", "OPERATION_OP_INVALID", t, e, n);
	if (typeof e.path != "string") throw new I("Operation `path` property is not a string", "OPERATION_PATH_INVALID", t, e, n);
	if (e.path.indexOf("/") !== 0 && e.path.length > 0) throw new I("Operation `path` property must start with \"/\"", "OPERATION_PATH_INVALID", t, e, n);
	if ((e.op === "move" || e.op === "copy") && typeof e.from != "string") throw new I("Operation `from` property is not present (applicable in `move` and `copy` operations)", "OPERATION_FROM_REQUIRED", t, e, n);
	if ((e.op === "add" || e.op === "replace" || e.op === "test") && e.value === void 0) throw new I("Operation `value` property is not present (applicable in `add`, `replace` and `test` operations)", "OPERATION_VALUE_REQUIRED", t, e, n);
	if ((e.op === "add" || e.op === "replace" || e.op === "test") && Bn(e.value)) throw new I("Operation `value` property is not present (applicable in `add`, `replace` and `test` operations)", "OPERATION_VALUE_CANNOT_CONTAIN_UNDEFINED", t, e, n);
	if (n) {
		if (e.op == "add") {
			var i = e.path.split("/").length, a = r.split("/").length;
			if (i !== a + 1 && i !== a) throw new I("Cannot perform an `add` operation at the desired path", "OPERATION_PATH_CANNOT_ADD", t, e, n);
		} else if (e.op === "replace" || e.op === "remove" || e.op === "_get") {
			if (e.path !== r) throw new I("Cannot perform the operation at a path that does not exist", "OPERATION_PATH_UNRESOLVABLE", t, e, n);
		} else if (e.op === "move" || e.op === "copy") {
			var o = Qn([{
				op: "_get",
				path: e.from,
				value: void 0
			}], n);
			if (o && o.name === "OPERATION_PATH_UNRESOLVABLE") throw new I("Cannot perform the operation from a path that does not exist", "OPERATION_FROM_UNRESOLVABLE", t, e, n);
		}
	}
}
function Qn(e, t, n) {
	try {
		if (!Array.isArray(e)) throw new I("Patch sequence must be an array", "SEQUENCE_NOT_AN_ARRAY");
		if (t) Yn(F(t), F(e), n || !0);
		else {
			n ||= Zn;
			for (var r = 0; r < e.length; r++) n(e[r], r, t, void 0);
		}
	} catch (e) {
		if (e instanceof I) return e;
		throw e;
	}
}
function $n(e, t) {
	if (e === t) return !0;
	if (e && t && typeof e == "object" && typeof t == "object") {
		var n = Array.isArray(e), r = Array.isArray(t), i, a, o;
		if (n && r) {
			if (a = e.length, a != t.length) return !1;
			for (i = a; i-- !== 0;) if (!$n(e[i], t[i])) return !1;
			return !0;
		}
		if (n != r) return !1;
		var s = Object.keys(e);
		if (a = s.length, a !== Object.keys(t).length) return !1;
		for (i = a; i-- !== 0;) if (!t.hasOwnProperty(s[i])) return !1;
		for (i = a; i-- !== 0;) if (o = s[i], !$n(e[o], t[o])) return !1;
		return !0;
	}
	return e !== e && t !== t;
}
//#endregion
//#region node_modules/fast-json-patch/module/duplex.mjs
var er = /* @__PURE__ */ t({
	compare: () => dr,
	generate: () => lr,
	observe: () => cr,
	unobserve: () => sr
}), tr = /* @__PURE__ */ new WeakMap(), nr = function() {
	function e(e) {
		this.observers = /* @__PURE__ */ new Map(), this.obj = e;
	}
	return e;
}(), rr = function() {
	function e(e, t) {
		this.callback = e, this.observer = t;
	}
	return e;
}();
function ir(e) {
	return tr.get(e);
}
function ar(e, t) {
	return e.observers.get(t);
}
function or(e, t) {
	e.observers.delete(t.callback);
}
function sr(e, t) {
	t.unobserve();
}
function cr(e, t) {
	var n = [], r, i = ir(e);
	if (!i) i = new nr(e), tr.set(e, i);
	else {
		var a = ar(i, t);
		r = a && a.observer;
	}
	if (r) return r;
	if (r = {}, i.value = F(e), t) {
		r.callback = t, r.next = null;
		var o = function() {
			lr(r);
		}, s = function() {
			clearTimeout(r.next), r.next = setTimeout(o);
		};
		typeof window < "u" && (window.addEventListener("mouseup", s), window.addEventListener("keyup", s), window.addEventListener("mousedown", s), window.addEventListener("keydown", s), window.addEventListener("change", s));
	}
	return r.patches = n, r.object = e, r.unobserve = function() {
		lr(r), clearTimeout(r.next), or(i, r), typeof window < "u" && (window.removeEventListener("mouseup", s), window.removeEventListener("keyup", s), window.removeEventListener("mousedown", s), window.removeEventListener("keydown", s), window.removeEventListener("change", s));
	}, i.observers.set(t, new rr(t, r)), r;
}
function lr(e, t) {
	t === void 0 && (t = !1);
	var n = tr.get(e.object);
	ur(n.value, e.object, e.patches, "", t), e.patches.length && Yn(n.value, e.patches);
	var r = e.patches;
	return r.length > 0 && (e.patches = [], e.callback && e.callback(r)), r;
}
function ur(e, t, n, r, i) {
	if (t !== e) {
		typeof t.toJSON == "function" && (t = t.toJSON());
		for (var a = In(t), o = In(e), s = !1, c = o.length - 1; c >= 0; c--) {
			var l = o[c], u = e[l];
			if (Fn(t, l) && !(t[l] === void 0 && u !== void 0 && Array.isArray(t) === !1)) {
				var d = t[l];
				typeof u == "object" && u && typeof d == "object" && d && Array.isArray(u) === Array.isArray(d) ? ur(u, d, n, r + "/" + Rn(l), i) : u !== d && (i && n.push({
					op: "test",
					path: r + "/" + Rn(l),
					value: F(u)
				}), n.push({
					op: "replace",
					path: r + "/" + Rn(l),
					value: F(d)
				}));
			} else Array.isArray(e) === Array.isArray(t) ? (i && n.push({
				op: "test",
				path: r + "/" + Rn(l),
				value: F(u)
			}), n.push({
				op: "remove",
				path: r + "/" + Rn(l)
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
			!Fn(e, l) && t[l] !== void 0 && n.push({
				op: "add",
				path: r + "/" + Rn(l),
				value: F(t[l])
			});
		}
	}
}
function dr(e, t, n) {
	n === void 0 && (n = !1);
	var r = [];
	return ur(e, t, r, "", n), r;
}
//#endregion
//#region node_modules/fast-json-patch/index.mjs
var fr = Object.assign({}, Un, er, {
	JsonPatchError: Hn,
	deepClone: F,
	escapePathComponent: Rn,
	unescapePathComponent: zn
}), pr = function(e, t) {
	return pr = Object.setPrototypeOf || { __proto__: [] } instanceof Array && function(e, t) {
		e.__proto__ = t;
	} || function(e, t) {
		for (var n in t) Object.prototype.hasOwnProperty.call(t, n) && (e[n] = t[n]);
	}, pr(e, t);
};
function mr(e, t) {
	if (typeof t != "function" && t !== null) throw TypeError("Class extends value " + String(t) + " is not a constructor or null");
	pr(e, t);
	function n() {
		this.constructor = e;
	}
	e.prototype = t === null ? Object.create(t) : (n.prototype = t.prototype, new n());
}
function hr(e, t, n, r) {
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
function gr(e, t) {
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
function _r(e) {
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
function vr(e, t) {
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
function yr(e, t, n) {
	if (n || arguments.length === 2) for (var r = 0, i = t.length, a; r < i; r++) (a || !(r in t)) && (a ||= Array.prototype.slice.call(t, 0, r), a[r] = t[r]);
	return e.concat(a || Array.prototype.slice.call(t));
}
function br(e) {
	return this instanceof br ? (this.v = e, this) : new br(e);
}
function xr(e, t, n) {
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
		e.value instanceof br ? Promise.resolve(e.value.v).then(u, d) : f(a[0][2], e);
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
function Sr(e) {
	if (!Symbol.asyncIterator) throw TypeError("Symbol.asyncIterator is not defined.");
	var t = e[Symbol.asyncIterator], n;
	return t ? t.call(e) : (e = typeof _r == "function" ? _r(e) : e[Symbol.iterator](), n = {}, r("next"), r("throw"), r("return"), n[Symbol.asyncIterator] = function() {
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
function L(e) {
	return typeof e == "function";
}
//#endregion
//#region node_modules/rxjs/dist/esm5/internal/util/createErrorClass.js
function Cr(e) {
	var t = e(function(e) {
		Error.call(e), e.stack = (/* @__PURE__ */ Error()).stack;
	});
	return t.prototype = Object.create(Error.prototype), t.prototype.constructor = t, t;
}
//#endregion
//#region node_modules/rxjs/dist/esm5/internal/util/UnsubscriptionError.js
var wr = Cr(function(e) {
	return function(t) {
		e(this), this.message = t ? t.length + " errors occurred during unsubscription:\n" + t.map(function(e, t) {
			return t + 1 + ") " + e.toString();
		}).join("\n  ") : "", this.name = "UnsubscriptionError", this.errors = t;
	};
});
//#endregion
//#region node_modules/rxjs/dist/esm5/internal/util/arrRemove.js
function Tr(e, t) {
	if (e) {
		var n = e.indexOf(t);
		0 <= n && e.splice(n, 1);
	}
}
//#endregion
//#region node_modules/rxjs/dist/esm5/internal/Subscription.js
var Er = function() {
	function e(e) {
		this.initialTeardown = e, this.closed = !1, this._parentage = null, this._finalizers = null;
	}
	return e.prototype.unsubscribe = function() {
		var e, t, n, r, i;
		if (!this.closed) {
			this.closed = !0;
			var a = this._parentage;
			if (a) if (this._parentage = null, Array.isArray(a)) try {
				for (var o = _r(a), s = o.next(); !s.done; s = o.next()) s.value.remove(this);
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
			if (L(c)) try {
				c();
			} catch (e) {
				i = e instanceof wr ? e.errors : [e];
			}
			var l = this._finalizers;
			if (l) {
				this._finalizers = null;
				try {
					for (var u = _r(l), d = u.next(); !d.done; d = u.next()) {
						var f = d.value;
						try {
							kr(f);
						} catch (e) {
							i ??= [], e instanceof wr ? i = yr(yr([], vr(i)), vr(e.errors)) : i.push(e);
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
			if (i) throw new wr(i);
		}
	}, e.prototype.add = function(t) {
		if (t && t !== this) if (this.closed) kr(t);
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
		t === e ? this._parentage = null : Array.isArray(t) && Tr(t, e);
	}, e.prototype.remove = function(t) {
		var n = this._finalizers;
		n && Tr(n, t), t instanceof e && t._removeParent(this);
	}, e.EMPTY = (function() {
		var t = new e();
		return t.closed = !0, t;
	})(), e;
}(), Dr = Er.EMPTY;
function Or(e) {
	return e instanceof Er || e && "closed" in e && L(e.remove) && L(e.add) && L(e.unsubscribe);
}
function kr(e) {
	L(e) ? e() : e.unsubscribe();
}
//#endregion
//#region node_modules/rxjs/dist/esm5/internal/config.js
var Ar = {
	onUnhandledError: null,
	onStoppedNotification: null,
	Promise: void 0,
	useDeprecatedSynchronousErrorHandling: !1,
	useDeprecatedNextContext: !1
}, jr = {
	setTimeout: function(e, t) {
		var n = [...arguments].slice(2), r = jr.delegate;
		return r?.setTimeout ? r.setTimeout.apply(r, yr([e, t], vr(n))) : setTimeout.apply(void 0, yr([e, t], vr(n)));
	},
	clearTimeout: function(e) {
		return (jr.delegate?.clearTimeout || clearTimeout)(e);
	},
	delegate: void 0
};
//#endregion
//#region node_modules/rxjs/dist/esm5/internal/util/reportUnhandledError.js
function Mr(e) {
	jr.setTimeout(function() {
		var t = Ar.onUnhandledError;
		if (t) t(e);
		else throw e;
	});
}
//#endregion
//#region node_modules/rxjs/dist/esm5/internal/util/noop.js
function Nr() {}
//#endregion
//#region node_modules/rxjs/dist/esm5/internal/NotificationFactories.js
var Pr = (function() {
	return Lr("C", void 0, void 0);
})();
function Fr(e) {
	return Lr("E", void 0, e);
}
function Ir(e) {
	return Lr("N", e, void 0);
}
function Lr(e, t, n) {
	return {
		kind: e,
		value: t,
		error: n
	};
}
//#endregion
//#region node_modules/rxjs/dist/esm5/internal/util/errorContext.js
var Rr = null;
function zr(e) {
	if (Ar.useDeprecatedSynchronousErrorHandling) {
		var t = !Rr;
		if (t && (Rr = {
			errorThrown: !1,
			error: null
		}), e(), t) {
			var n = Rr, r = n.errorThrown, i = n.error;
			if (Rr = null, r) throw i;
		}
	} else e();
}
function Br(e) {
	Ar.useDeprecatedSynchronousErrorHandling && Rr && (Rr.errorThrown = !0, Rr.error = e);
}
//#endregion
//#region node_modules/rxjs/dist/esm5/internal/Subscriber.js
var Vr = function(e) {
	mr(t, e);
	function t(t) {
		var n = e.call(this) || this;
		return n.isStopped = !1, t ? (n.destination = t, Or(t) && t.add(n)) : n.destination = Yr, n;
	}
	return t.create = function(e, t, n) {
		return new Gr(e, t, n);
	}, t.prototype.next = function(e) {
		this.isStopped ? Jr(Ir(e), this) : this._next(e);
	}, t.prototype.error = function(e) {
		this.isStopped ? Jr(Fr(e), this) : (this.isStopped = !0, this._error(e));
	}, t.prototype.complete = function() {
		this.isStopped ? Jr(Pr, this) : (this.isStopped = !0, this._complete());
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
}(Er), Hr = Function.prototype.bind;
function Ur(e, t) {
	return Hr.call(e, t);
}
var Wr = function() {
	function e(e) {
		this.partialObserver = e;
	}
	return e.prototype.next = function(e) {
		var t = this.partialObserver;
		if (t.next) try {
			t.next(e);
		} catch (e) {
			Kr(e);
		}
	}, e.prototype.error = function(e) {
		var t = this.partialObserver;
		if (t.error) try {
			t.error(e);
		} catch (e) {
			Kr(e);
		}
		else Kr(e);
	}, e.prototype.complete = function() {
		var e = this.partialObserver;
		if (e.complete) try {
			e.complete();
		} catch (e) {
			Kr(e);
		}
	}, e;
}(), Gr = function(e) {
	mr(t, e);
	function t(t, n, r) {
		var i = e.call(this) || this, a;
		if (L(t) || !t) a = {
			next: t ?? void 0,
			error: n ?? void 0,
			complete: r ?? void 0
		};
		else {
			var o;
			i && Ar.useDeprecatedNextContext ? (o = Object.create(t), o.unsubscribe = function() {
				return i.unsubscribe();
			}, a = {
				next: t.next && Ur(t.next, o),
				error: t.error && Ur(t.error, o),
				complete: t.complete && Ur(t.complete, o)
			}) : a = t;
		}
		return i.destination = new Wr(a), i;
	}
	return t;
}(Vr);
function Kr(e) {
	Ar.useDeprecatedSynchronousErrorHandling ? Br(e) : Mr(e);
}
function qr(e) {
	throw e;
}
function Jr(e, t) {
	var n = Ar.onStoppedNotification;
	n && jr.setTimeout(function() {
		return n(e, t);
	});
}
var Yr = {
	closed: !0,
	next: Nr,
	error: qr,
	complete: Nr
}, Xr = (function() {
	return typeof Symbol == "function" && Symbol.observable || "@@observable";
})();
//#endregion
//#region node_modules/rxjs/dist/esm5/internal/util/identity.js
function Zr(e) {
	return e;
}
//#endregion
//#region node_modules/rxjs/dist/esm5/internal/util/pipe.js
function Qr() {
	return $r([...arguments]);
}
function $r(e) {
	return e.length === 0 ? Zr : e.length === 1 ? e[0] : function(t) {
		return e.reduce(function(e, t) {
			return t(e);
		}, t);
	};
}
//#endregion
//#region node_modules/rxjs/dist/esm5/internal/Observable.js
var R = function() {
	function e(e) {
		e && (this._subscribe = e);
	}
	return e.prototype.lift = function(t) {
		var n = new e();
		return n.source = this, n.operator = t, n;
	}, e.prototype.subscribe = function(e, t, n) {
		var r = this, i = ni(e) ? e : new Gr(e, t, n);
		return zr(function() {
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
		return t = ei(t), new t(function(t, r) {
			var i = new Gr({
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
	}, e.prototype[Xr] = function() {
		return this;
	}, e.prototype.pipe = function() {
		return $r([...arguments])(this);
	}, e.prototype.toPromise = function(e) {
		var t = this;
		return e = ei(e), new e(function(e, n) {
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
function ei(e) {
	return e ?? Ar.Promise ?? Promise;
}
function ti(e) {
	return e && L(e.next) && L(e.error) && L(e.complete);
}
function ni(e) {
	return e && e instanceof Vr || ti(e) && Or(e);
}
//#endregion
//#region node_modules/rxjs/dist/esm5/internal/util/lift.js
function ri(e) {
	return L(e?.lift);
}
function z(e) {
	return function(t) {
		if (ri(t)) return t.lift(function(t) {
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
function B(e, t, n, r, i) {
	return new ii(e, t, n, r, i);
}
var ii = function(e) {
	mr(t, e);
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
}(Vr), ai = Cr(function(e) {
	return function() {
		e(this), this.name = "ObjectUnsubscribedError", this.message = "object unsubscribed";
	};
}), oi = function(e) {
	mr(t, e);
	function t() {
		var t = e.call(this) || this;
		return t.closed = !1, t.currentObservers = null, t.observers = [], t.isStopped = !1, t.hasError = !1, t.thrownError = null, t;
	}
	return t.prototype.lift = function(e) {
		var t = new si(this, this);
		return t.operator = e, t;
	}, t.prototype._throwIfClosed = function() {
		if (this.closed) throw new ai();
	}, t.prototype.next = function(e) {
		var t = this;
		zr(function() {
			var n, r;
			if (t._throwIfClosed(), !t.isStopped) {
				t.currentObservers ||= Array.from(t.observers);
				try {
					for (var i = _r(t.currentObservers), a = i.next(); !a.done; a = i.next()) a.value.next(e);
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
		zr(function() {
			if (t._throwIfClosed(), !t.isStopped) {
				t.hasError = t.isStopped = !0, t.thrownError = e;
				for (var n = t.observers; n.length;) n.shift().error(e);
			}
		});
	}, t.prototype.complete = function() {
		var e = this;
		zr(function() {
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
		return r || i ? Dr : (this.currentObservers = null, a.push(e), new Er(function() {
			t.currentObservers = null, Tr(a, e);
		}));
	}, t.prototype._checkFinalizedStatuses = function(e) {
		var t = this, n = t.hasError, r = t.thrownError, i = t.isStopped;
		n ? e.error(r) : i && e.complete();
	}, t.prototype.asObservable = function() {
		var e = new R();
		return e.source = this, e;
	}, t.create = function(e, t) {
		return new si(e, t);
	}, t;
}(R), si = function(e) {
	mr(t, e);
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
		return this.source?.subscribe(e) ?? Dr;
	}, t;
}(oi), ci = {
	now: function() {
		return (ci.delegate || Date).now();
	},
	delegate: void 0
}, li = function(e) {
	mr(t, e);
	function t(t, n, r) {
		t === void 0 && (t = Infinity), n === void 0 && (n = Infinity), r === void 0 && (r = ci);
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
}(oi), ui = new R(function(e) {
	return e.complete();
});
//#endregion
//#region node_modules/rxjs/dist/esm5/internal/util/isScheduler.js
function di(e) {
	return e && L(e.schedule);
}
//#endregion
//#region node_modules/rxjs/dist/esm5/internal/util/args.js
function fi(e) {
	return e[e.length - 1];
}
function pi(e) {
	return di(fi(e)) ? e.pop() : void 0;
}
//#endregion
//#region node_modules/rxjs/dist/esm5/internal/util/isArrayLike.js
var mi = (function(e) {
	return e && typeof e.length == "number" && typeof e != "function";
});
//#endregion
//#region node_modules/rxjs/dist/esm5/internal/util/isPromise.js
function hi(e) {
	return L(e?.then);
}
//#endregion
//#region node_modules/rxjs/dist/esm5/internal/util/isInteropObservable.js
function gi(e) {
	return L(e[Xr]);
}
//#endregion
//#region node_modules/rxjs/dist/esm5/internal/util/isAsyncIterable.js
function _i(e) {
	return Symbol.asyncIterator && L(e?.[Symbol.asyncIterator]);
}
//#endregion
//#region node_modules/rxjs/dist/esm5/internal/util/throwUnobservableError.js
function vi(e) {
	return /* @__PURE__ */ TypeError("You provided " + (typeof e == "object" && e ? "an invalid object" : "'" + e + "'") + " where a stream was expected. You can provide an Observable, Promise, ReadableStream, Array, AsyncIterable, or Iterable.");
}
//#endregion
//#region node_modules/rxjs/dist/esm5/internal/symbol/iterator.js
function yi() {
	return typeof Symbol != "function" || !Symbol.iterator ? "@@iterator" : Symbol.iterator;
}
var bi = yi();
//#endregion
//#region node_modules/rxjs/dist/esm5/internal/util/isIterable.js
function xi(e) {
	return L(e?.[bi]);
}
//#endregion
//#region node_modules/rxjs/dist/esm5/internal/util/isReadableStreamLike.js
function Si(e) {
	return xr(this, arguments, function() {
		var t, n, r, i;
		return gr(this, function(a) {
			switch (a.label) {
				case 0: t = e.getReader(), a.label = 1;
				case 1: a.trys.push([
					1,
					,
					9,
					10
				]), a.label = 2;
				case 2: return [4, br(t.read())];
				case 3: return n = a.sent(), r = n.value, i = n.done, i ? [4, br(void 0)] : [3, 5];
				case 4: return [2, a.sent()];
				case 5: return [4, br(r)];
				case 6: return [4, a.sent()];
				case 7: return a.sent(), [3, 2];
				case 8: return [3, 10];
				case 9: return t.releaseLock(), [7];
				case 10: return [2];
			}
		});
	});
}
function Ci(e) {
	return L(e?.getReader);
}
//#endregion
//#region node_modules/rxjs/dist/esm5/internal/observable/innerFrom.js
function wi(e) {
	if (e instanceof R) return e;
	if (e != null) {
		if (gi(e)) return Ti(e);
		if (mi(e)) return Ei(e);
		if (hi(e)) return Di(e);
		if (_i(e)) return ki(e);
		if (xi(e)) return Oi(e);
		if (Ci(e)) return Ai(e);
	}
	throw vi(e);
}
function Ti(e) {
	return new R(function(t) {
		var n = e[Xr]();
		if (L(n.subscribe)) return n.subscribe(t);
		throw TypeError("Provided object does not correctly implement Symbol.observable");
	});
}
function Ei(e) {
	return new R(function(t) {
		for (var n = 0; n < e.length && !t.closed; n++) t.next(e[n]);
		t.complete();
	});
}
function Di(e) {
	return new R(function(t) {
		e.then(function(e) {
			t.closed || (t.next(e), t.complete());
		}, function(e) {
			return t.error(e);
		}).then(null, Mr);
	});
}
function Oi(e) {
	return new R(function(t) {
		var n, r;
		try {
			for (var i = _r(e), a = i.next(); !a.done; a = i.next()) {
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
function ki(e) {
	return new R(function(t) {
		ji(e, t).catch(function(e) {
			return t.error(e);
		});
	});
}
function Ai(e) {
	return ki(Si(e));
}
function ji(e, t) {
	var n, r, i, a;
	return hr(this, void 0, void 0, function() {
		var o, s;
		return gr(this, function(c) {
			switch (c.label) {
				case 0: c.trys.push([
					0,
					5,
					6,
					11
				]), n = Sr(e), c.label = 1;
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
function Mi(e, t, n, r, i) {
	r === void 0 && (r = 0), i === void 0 && (i = !1);
	var a = t.schedule(function() {
		n(), i ? e.add(this.schedule(null, r)) : this.unsubscribe();
	}, r);
	if (e.add(a), !i) return a;
}
//#endregion
//#region node_modules/rxjs/dist/esm5/internal/operators/observeOn.js
function Ni(e, t) {
	return t === void 0 && (t = 0), z(function(n, r) {
		n.subscribe(B(r, function(n) {
			return Mi(r, e, function() {
				return r.next(n);
			}, t);
		}, function() {
			return Mi(r, e, function() {
				return r.complete();
			}, t);
		}, function(n) {
			return Mi(r, e, function() {
				return r.error(n);
			}, t);
		}));
	});
}
//#endregion
//#region node_modules/rxjs/dist/esm5/internal/operators/subscribeOn.js
function Pi(e, t) {
	return t === void 0 && (t = 0), z(function(n, r) {
		r.add(e.schedule(function() {
			return n.subscribe(r);
		}, t));
	});
}
//#endregion
//#region node_modules/rxjs/dist/esm5/internal/scheduled/scheduleObservable.js
function Fi(e, t) {
	return wi(e).pipe(Pi(t), Ni(t));
}
//#endregion
//#region node_modules/rxjs/dist/esm5/internal/scheduled/schedulePromise.js
function Ii(e, t) {
	return wi(e).pipe(Pi(t), Ni(t));
}
//#endregion
//#region node_modules/rxjs/dist/esm5/internal/scheduled/scheduleArray.js
function Li(e, t) {
	return new R(function(n) {
		var r = 0;
		return t.schedule(function() {
			r === e.length ? n.complete() : (n.next(e[r++]), n.closed || this.schedule());
		});
	});
}
//#endregion
//#region node_modules/rxjs/dist/esm5/internal/scheduled/scheduleIterable.js
function Ri(e, t) {
	return new R(function(n) {
		var r;
		return Mi(n, t, function() {
			r = e[bi](), Mi(n, t, function() {
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
			return L(r?.return) && r.return();
		};
	});
}
//#endregion
//#region node_modules/rxjs/dist/esm5/internal/scheduled/scheduleAsyncIterable.js
function zi(e, t) {
	if (!e) throw Error("Iterable cannot be null");
	return new R(function(n) {
		Mi(n, t, function() {
			var r = e[Symbol.asyncIterator]();
			Mi(n, t, function() {
				r.next().then(function(e) {
					e.done ? n.complete() : n.next(e.value);
				});
			}, 0, !0);
		});
	});
}
//#endregion
//#region node_modules/rxjs/dist/esm5/internal/scheduled/scheduleReadableStreamLike.js
function Bi(e, t) {
	return zi(Si(e), t);
}
//#endregion
//#region node_modules/rxjs/dist/esm5/internal/scheduled/scheduled.js
function Vi(e, t) {
	if (e != null) {
		if (gi(e)) return Fi(e, t);
		if (mi(e)) return Li(e, t);
		if (hi(e)) return Ii(e, t);
		if (_i(e)) return zi(e, t);
		if (xi(e)) return Ri(e, t);
		if (Ci(e)) return Bi(e, t);
	}
	throw vi(e);
}
//#endregion
//#region node_modules/rxjs/dist/esm5/internal/observable/from.js
function Hi(e, t) {
	return t ? Vi(e, t) : wi(e);
}
//#endregion
//#region node_modules/rxjs/dist/esm5/internal/observable/of.js
function V() {
	var e = [...arguments];
	return Hi(e, pi(e));
}
//#endregion
//#region node_modules/rxjs/dist/esm5/internal/observable/throwError.js
function H(e, t) {
	var n = L(e) ? e : function() {
		return e;
	}, r = function(e) {
		return e.error(n());
	};
	return new R(t ? function(e) {
		return t.schedule(r, 0, e);
	} : r);
}
//#endregion
//#region node_modules/rxjs/dist/esm5/internal/util/EmptyError.js
var Ui = Cr(function(e) {
	return function() {
		e(this), this.name = "EmptyError", this.message = "no elements in sequence";
	};
});
//#endregion
//#region node_modules/rxjs/dist/esm5/internal/lastValueFrom.js
function Wi(e, t) {
	var n = typeof t == "object";
	return new Promise(function(r, i) {
		var a = !1, o;
		e.subscribe({
			next: function(e) {
				o = e, a = !0;
			},
			error: i,
			complete: function() {
				a ? r(o) : n ? r(t.defaultValue) : i(new Ui());
			}
		});
	});
}
//#endregion
//#region node_modules/rxjs/dist/esm5/internal/operators/map.js
function Gi(e, t) {
	return z(function(n, r) {
		var i = 0;
		n.subscribe(B(r, function(n) {
			r.next(e.call(t, n, i++));
		}));
	});
}
//#endregion
//#region node_modules/rxjs/dist/esm5/internal/operators/mergeInternals.js
function Ki(e, t, n, r, i, a, o, s) {
	var c = [], l = 0, u = 0, d = !1, f = function() {
		d && !c.length && !l && t.complete();
	}, p = function(e) {
		return l < r ? m(e) : c.push(e);
	}, m = function(e) {
		a && t.next(e), l++;
		var s = !1;
		wi(n(e, u++)).subscribe(B(t, function(e) {
			i?.(e), a ? p(e) : t.next(e);
		}, function() {
			s = !0;
		}, void 0, function() {
			if (s) try {
				l--;
				for (var e = function() {
					var e = c.shift();
					o ? Mi(t, o, function() {
						return m(e);
					}) : m(e);
				}; c.length && l < r;) e();
				f();
			} catch (e) {
				t.error(e);
			}
		}));
	};
	return e.subscribe(B(t, p, function() {
		d = !0, f();
	})), function() {
		s?.();
	};
}
//#endregion
//#region node_modules/rxjs/dist/esm5/internal/operators/mergeMap.js
function qi(e, t, n) {
	return n === void 0 && (n = Infinity), L(t) ? qi(function(n, r) {
		return Gi(function(e, i) {
			return t(n, e, r, i);
		})(wi(e(n, r)));
	}, n) : (typeof t == "number" && (n = t), z(function(t, r) {
		return Ki(t, r, e, n);
	}));
}
//#endregion
//#region node_modules/rxjs/dist/esm5/internal/operators/mergeAll.js
function Ji(e) {
	return e === void 0 && (e = Infinity), qi(Zr, e);
}
//#endregion
//#region node_modules/rxjs/dist/esm5/internal/observable/defer.js
function Yi(e) {
	return new R(function(t) {
		wi(e()).subscribe(t);
	});
}
//#endregion
//#region node_modules/rxjs/dist/esm5/internal/operators/catchError.js
function Xi(e) {
	return z(function(t, n) {
		var r = null, i = !1, a;
		r = t.subscribe(B(n, void 0, void 0, function(o) {
			a = wi(e(o, Xi(e)(t))), r ? (r.unsubscribe(), r = null, a.subscribe(n)) : i = !0;
		})), i && (r.unsubscribe(), r = null, a.subscribe(n));
	});
}
//#endregion
//#region node_modules/rxjs/dist/esm5/internal/operators/concatMap.js
function Zi(e, t) {
	return L(t) ? qi(e, t, 1) : qi(e, 1);
}
//#endregion
//#region node_modules/rxjs/dist/esm5/internal/operators/defaultIfEmpty.js
function Qi(e) {
	return z(function(t, n) {
		var r = !1;
		t.subscribe(B(n, function(e) {
			r = !0, n.next(e);
		}, function() {
			r || n.next(e), n.complete();
		}));
	});
}
//#endregion
//#region node_modules/rxjs/dist/esm5/internal/operators/finalize.js
function $i(e) {
	return z(function(t, n) {
		try {
			t.subscribe(n);
		} finally {
			n.add(e);
		}
	});
}
//#endregion
//#region node_modules/rxjs/dist/esm5/internal/operators/switchMap.js
function ea(e, t) {
	return z(function(n, r) {
		var i = null, a = 0, o = !1, s = function() {
			return o && !i && r.complete();
		};
		n.subscribe(B(r, function(n) {
			i?.unsubscribe();
			var o = 0, c = a++;
			wi(e(n, c)).subscribe(i = B(r, function(e) {
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
function ta(e) {
	return z(function(t, n) {
		wi(e).subscribe(B(n, function() {
			return n.complete();
		}, Nr)), !n.closed && t.subscribe(n);
	});
}
//#endregion
//#region node_modules/rxjs/dist/esm5/internal/operators/tap.js
function na(e, t, n) {
	var r = L(e) || t || n ? {
		next: e,
		error: t,
		complete: n
	} : e;
	return r ? z(function(e, t) {
		var n;
		(n = r.subscribe) == null || n.call(r);
		var i = !0;
		e.subscribe(B(t, function(e) {
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
	}) : Zr;
}
//#endregion
//#region node_modules/untruncate-json/dist/esm/index.js
function ra(e) {
	return " \r\n	".indexOf(e) >= 0;
}
function ia(e) {
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
				m === "]" ? u() : ra(m) || (l("collectionItem"), s("arrayNeedsComma"), f(m));
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
				ra(m) || (l("collectionItem"), s("objectNeedsComma"), f(m));
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
function aa() {
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
function oa(e, t, n) {
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
var sa = 4294967296;
function ca(e) {
	let t = e[0] === "-";
	t && (e = e.slice(1));
	let n = 1e6, r = 0, i = 0;
	function a(t, a) {
		let o = Number(e.slice(t, a));
		i *= n, r = r * n + o, r >= sa && (i += r / sa | 0, r %= sa);
	}
	return a(-24, -18), a(-18, -12), a(-12, -6), a(-6), t ? pa(r, i) : fa(r, i);
}
function la(e, t) {
	let n = fa(e, t), r = n.hi & 2147483648;
	r && (n = pa(n.lo, n.hi));
	let i = ua(n.lo, n.hi);
	return r ? "-" + i : i;
}
function ua(e, t) {
	if ({lo: e, hi: t} = da(e, t), t <= 2097151) return String(sa * t + e);
	let n = e & 16777215, r = (e >>> 24 | t << 8) & 16777215, i = t >> 16 & 65535, a = n + r * 6777216 + i * 6710656, o = r + i * 8147497, s = i * 2, c = 1e7;
	return a >= c && (o += Math.floor(a / c), a %= c), o >= c && (s += Math.floor(o / c), o %= c), s.toString() + ma(o) + ma(a);
}
function da(e, t) {
	return {
		lo: e >>> 0,
		hi: t >>> 0
	};
}
function fa(e, t) {
	return {
		lo: e | 0,
		hi: t | 0
	};
}
function pa(e, t) {
	return t = ~t, e ? e = ~e + 1 : t += 1, fa(e, t);
}
var ma = (e) => {
	let t = String(e);
	return "0000000".slice(t.length) + t;
};
function ha(e, t) {
	if (e >= 0) {
		for (; e > 127;) t.push(e & 127 | 128), e >>>= 7;
		t.push(e);
	} else {
		for (let n = 0; n < 9; n++) t.push(e & 127 | 128), e >>= 7;
		t.push(1);
	}
}
function ga() {
	let e = this.buf[this.pos++], t = e & 127;
	if (!(e & 128) || (e = this.buf[this.pos++], t |= (e & 127) << 7, !(e & 128)) || (e = this.buf[this.pos++], t |= (e & 127) << 14, !(e & 128)) || (e = this.buf[this.pos++], t |= (e & 127) << 21, !(e & 128))) return this.assertBounds(), t;
	e = this.buf[this.pos++], t |= (e & 15) << 28;
	for (let t = 5; e & 128 && t < 10; t++) e = this.buf[this.pos++];
	if (e & 128) throw Error("invalid varint");
	return this.assertBounds(), t >>> 0;
}
//#endregion
//#region node_modules/@bufbuild/protobuf/dist/esm/proto-int64.js
var U = /* @__PURE__ */ _a();
function _a() {
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
			return typeof e != "string" && (e = e.toString()), va(e), e;
		},
		uParse(e) {
			return typeof e != "string" && (e = e.toString()), ya(e), e;
		},
		enc(e) {
			return typeof e != "string" && (e = e.toString()), va(e), ca(e);
		},
		uEnc(e) {
			return typeof e != "string" && (e = e.toString()), ya(e), ca(e);
		},
		dec(e, t) {
			return la(e, t);
		},
		uDec(e, t) {
			return ua(e, t);
		}
	};
}
function va(e) {
	if (!/^-?[0-9]+$/.test(e)) throw Error("invalid int64: " + e);
}
function ya(e) {
	if (!/^[0-9]+$/.test(e)) throw Error("invalid uint64: " + e);
}
//#endregion
//#region node_modules/@bufbuild/protobuf/dist/esm/wire/text-encoding.js
var ba = Symbol.for("@bufbuild/protobuf/text-encoding");
function xa() {
	if (globalThis[ba] == null) {
		let e = new globalThis.TextEncoder(), t = new globalThis.TextDecoder(), n;
		globalThis[ba] = {
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
	return globalThis[ba];
}
//#endregion
//#region node_modules/@bufbuild/protobuf/dist/esm/wire/binary-encoding.js
var Sa;
(function(e) {
	e[e.Varint = 0] = "Varint", e[e.Bit64 = 1] = "Bit64", e[e.LengthDelimited = 2] = "LengthDelimited", e[e.StartGroup = 3] = "StartGroup", e[e.EndGroup = 4] = "EndGroup", e[e.Bit32 = 5] = "Bit32";
})(Sa ||= {});
var W = class {
	constructor(e = xa().encodeUtf8) {
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
		for (wa(e); e > 127;) this.buf.push(e & 127 | 128), e >>>= 7;
		return this.buf.push(e), this;
	}
	int32(e) {
		return Ca(e), ha(e, this.buf), this;
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
		Ta(e);
		let t = new Uint8Array(4);
		return new DataView(t.buffer).setFloat32(0, e, !0), this.raw(t);
	}
	double(e) {
		let t = new Uint8Array(8);
		return new DataView(t.buffer).setFloat64(0, e, !0), this.raw(t);
	}
	fixed32(e) {
		wa(e);
		let t = new Uint8Array(4);
		return new DataView(t.buffer).setUint32(0, e, !0), this.raw(t);
	}
	sfixed32(e) {
		Ca(e);
		let t = new Uint8Array(4);
		return new DataView(t.buffer).setInt32(0, e, !0), this.raw(t);
	}
	sint32(e) {
		return Ca(e), e = (e << 1 ^ e >> 31) >>> 0, ha(e, this.buf), this;
	}
	sfixed64(e) {
		let t = new Uint8Array(8), n = new DataView(t.buffer), r = U.enc(e);
		return n.setInt32(0, r.lo, !0), n.setInt32(4, r.hi, !0), this.raw(t);
	}
	fixed64(e) {
		let t = new Uint8Array(8), n = new DataView(t.buffer), r = U.uEnc(e);
		return n.setInt32(0, r.lo, !0), n.setInt32(4, r.hi, !0), this.raw(t);
	}
	int64(e) {
		let t = U.enc(e);
		return oa(t.lo, t.hi, this.buf), this;
	}
	sint64(e) {
		let t = U.enc(e), n = t.hi >> 31;
		return oa(t.lo << 1 ^ n, (t.hi << 1 | t.lo >>> 31) ^ n, this.buf), this;
	}
	uint64(e) {
		let t = U.uEnc(e);
		return oa(t.lo, t.hi, this.buf), this;
	}
}, G = class {
	constructor(e, t = xa().decodeUtf8) {
		this.decodeUtf8 = t, this.varint64 = aa, this.uint32 = ga, this.buf = e, this.len = e.length, this.pos = 0, this.view = new DataView(e.buffer, e.byteOffset, e.byteLength);
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
			case Sa.Varint:
				for (; this.buf[this.pos++] & 128;);
				break;
			case Sa.Bit64: this.pos += 4;
			case Sa.Bit32:
				this.pos += 4;
				break;
			case Sa.LengthDelimited:
				let n = this.uint32();
				this.pos += n;
				break;
			case Sa.StartGroup:
				for (;;) {
					let [e, n] = this.tag();
					if (n === Sa.EndGroup) {
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
		return U.dec(...this.varint64());
	}
	uint64() {
		return U.uDec(...this.varint64());
	}
	sint64() {
		let [e, t] = this.varint64(), n = -(e & 1);
		return e = (e >>> 1 | (t & 1) << 31) ^ n, t = t >>> 1 ^ n, U.dec(e, t);
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
		return U.uDec(this.sfixed32(), this.sfixed32());
	}
	sfixed64() {
		return U.dec(this.sfixed32(), this.sfixed32());
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
function Ca(e) {
	if (typeof e == "string") e = Number(e);
	else if (typeof e != "number") throw Error("invalid int32: " + typeof e);
	if (!Number.isInteger(e) || e > 2147483647 || e < -2147483648) throw Error("invalid int32: " + e);
}
function wa(e) {
	if (typeof e == "string") e = Number(e);
	else if (typeof e != "number") throw Error("invalid uint32: " + typeof e);
	if (!Number.isInteger(e) || e > 4294967295 || e < 0) throw Error("invalid uint32: " + e);
}
function Ta(e) {
	if (typeof e == "string") {
		let t = e;
		if (e = Number(e), Number.isNaN(e) && t !== "NaN") throw Error("invalid float32: " + t);
	} else if (typeof e != "number") throw Error("invalid float32: " + typeof e);
	if (Number.isFinite(e) && (e > 34028234663852886e22 || e < -34028234663852886e22)) throw Error("invalid float32: " + e);
}
//#endregion
//#region node_modules/@ag-ui/proto/dist/index.mjs
var Ea = /* @__PURE__ */ function(e) {
	return e[e.NULL_VALUE = 0] = "NULL_VALUE", e[e.UNRECOGNIZED = -1] = "UNRECOGNIZED", e;
}({});
function Da() {
	return { fields: {} };
}
var Oa = {
	encode(e, t = new W()) {
		return Object.entries(e.fields).forEach(([e, n]) => {
			n !== void 0 && Aa.encode({
				key: e,
				value: n
			}, t.uint32(10).fork()).join();
		}), t;
	},
	decode(e, t) {
		let n = e instanceof G ? e : new G(e), r = t === void 0 ? n.len : n.pos + t, i = Da();
		for (; n.pos < r;) {
			let e = n.uint32();
			switch (e >>> 3) {
				case 1: {
					if (e !== 10) break;
					let t = Aa.decode(n, n.uint32());
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
		return Oa.fromPartial(e ?? {});
	},
	fromPartial(e) {
		let t = Da();
		return t.fields = Object.entries(e.fields ?? {}).reduce((e, [t, n]) => (n !== void 0 && (e[t] = n), e), {}), t;
	},
	wrap(e) {
		let t = Da();
		if (e !== void 0) for (let n of Object.keys(e)) t.fields[n] = e[n];
		return t;
	},
	unwrap(e) {
		let t = {};
		if (e.fields) for (let n of Object.keys(e.fields)) t[n] = e.fields[n];
		return t;
	}
};
function ka() {
	return {
		key: "",
		value: void 0
	};
}
var Aa = {
	encode(e, t = new W()) {
		return e.key !== "" && t.uint32(10).string(e.key), e.value !== void 0 && K.encode(K.wrap(e.value), t.uint32(18).fork()).join(), t;
	},
	decode(e, t) {
		let n = e instanceof G ? e : new G(e), r = t === void 0 ? n.len : n.pos + t, i = ka();
		for (; n.pos < r;) {
			let e = n.uint32();
			switch (e >>> 3) {
				case 1:
					if (e !== 10) break;
					i.key = n.string();
					continue;
				case 2:
					if (e !== 18) break;
					i.value = K.unwrap(K.decode(n, n.uint32()));
					continue;
			}
			if ((e & 7) == 4 || e === 0) break;
			n.skip(e & 7);
		}
		return i;
	},
	create(e) {
		return Aa.fromPartial(e ?? {});
	},
	fromPartial(e) {
		let t = ka();
		return t.key = e.key ?? "", t.value = e.value ?? void 0, t;
	}
};
function ja() {
	return {
		nullValue: void 0,
		numberValue: void 0,
		stringValue: void 0,
		boolValue: void 0,
		structValue: void 0,
		listValue: void 0
	};
}
var K = {
	encode(e, t = new W()) {
		return e.nullValue !== void 0 && t.uint32(8).int32(e.nullValue), e.numberValue !== void 0 && t.uint32(17).double(e.numberValue), e.stringValue !== void 0 && t.uint32(26).string(e.stringValue), e.boolValue !== void 0 && t.uint32(32).bool(e.boolValue), e.structValue !== void 0 && Oa.encode(Oa.wrap(e.structValue), t.uint32(42).fork()).join(), e.listValue !== void 0 && Na.encode(Na.wrap(e.listValue), t.uint32(50).fork()).join(), t;
	},
	decode(e, t) {
		let n = e instanceof G ? e : new G(e), r = t === void 0 ? n.len : n.pos + t, i = ja();
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
					i.structValue = Oa.unwrap(Oa.decode(n, n.uint32()));
					continue;
				case 6:
					if (e !== 50) break;
					i.listValue = Na.unwrap(Na.decode(n, n.uint32()));
					continue;
			}
			if ((e & 7) == 4 || e === 0) break;
			n.skip(e & 7);
		}
		return i;
	},
	create(e) {
		return K.fromPartial(e ?? {});
	},
	fromPartial(e) {
		let t = ja();
		return t.nullValue = e.nullValue ?? void 0, t.numberValue = e.numberValue ?? void 0, t.stringValue = e.stringValue ?? void 0, t.boolValue = e.boolValue ?? void 0, t.structValue = e.structValue ?? void 0, t.listValue = e.listValue ?? void 0, t;
	},
	wrap(e) {
		let t = ja();
		if (e === null) t.nullValue = Ea.NULL_VALUE;
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
function Ma() {
	return { values: [] };
}
var Na = {
	encode(e, t = new W()) {
		for (let n of e.values) K.encode(K.wrap(n), t.uint32(10).fork()).join();
		return t;
	},
	decode(e, t) {
		let n = e instanceof G ? e : new G(e), r = t === void 0 ? n.len : n.pos + t, i = Ma();
		for (; n.pos < r;) {
			let e = n.uint32();
			switch (e >>> 3) {
				case 1:
					if (e !== 10) break;
					i.values.push(K.unwrap(K.decode(n, n.uint32())));
					continue;
			}
			if ((e & 7) == 4 || e === 0) break;
			n.skip(e & 7);
		}
		return i;
	},
	create(e) {
		return Na.fromPartial(e ?? {});
	},
	fromPartial(e) {
		let t = Ma();
		return t.values = e.values?.map((e) => e) || [], t;
	},
	wrap(e) {
		let t = Ma();
		return t.values = e ?? [], t;
	},
	unwrap(e) {
		return e?.hasOwnProperty("values") && globalThis.Array.isArray(e.values) ? e.values : e;
	}
}, Pa = /* @__PURE__ */ function(e) {
	return e[e.ADD = 0] = "ADD", e[e.REMOVE = 1] = "REMOVE", e[e.REPLACE = 2] = "REPLACE", e[e.MOVE = 3] = "MOVE", e[e.COPY = 4] = "COPY", e[e.TEST = 5] = "TEST", e[e.UNRECOGNIZED = -1] = "UNRECOGNIZED", e;
}({});
function Fa() {
	return {
		op: 0,
		path: "",
		from: void 0,
		value: void 0
	};
}
var Ia = {
	encode(e, t = new W()) {
		return e.op !== 0 && t.uint32(8).int32(e.op), e.path !== "" && t.uint32(18).string(e.path), e.from !== void 0 && t.uint32(26).string(e.from), e.value !== void 0 && K.encode(K.wrap(e.value), t.uint32(34).fork()).join(), t;
	},
	decode(e, t) {
		let n = e instanceof G ? e : new G(e), r = t === void 0 ? n.len : n.pos + t, i = Fa();
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
					i.value = K.unwrap(K.decode(n, n.uint32()));
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
		return t.op = e.op ?? 0, t.path = e.path ?? "", t.from = e.from ?? void 0, t.value = e.value ?? void 0, t;
	}
};
function La() {
	return {
		id: "",
		type: "",
		function: void 0
	};
}
var Ra = {
	encode(e, t = new W()) {
		return e.id !== "" && t.uint32(10).string(e.id), e.type !== "" && t.uint32(18).string(e.type), e.function !== void 0 && Ba.encode(e.function, t.uint32(26).fork()).join(), t;
	},
	decode(e, t) {
		let n = e instanceof G ? e : new G(e), r = t === void 0 ? n.len : n.pos + t, i = La();
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
					i.function = Ba.decode(n, n.uint32());
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
		return t.id = e.id ?? "", t.type = e.type ?? "", t.function = e.function !== void 0 && e.function !== null ? Ba.fromPartial(e.function) : void 0, t;
	}
};
function za() {
	return {
		name: "",
		arguments: ""
	};
}
var Ba = {
	encode(e, t = new W()) {
		return e.name !== "" && t.uint32(10).string(e.name), e.arguments !== "" && t.uint32(18).string(e.arguments), t;
	},
	decode(e, t) {
		let n = e instanceof G ? e : new G(e), r = t === void 0 ? n.len : n.pos + t, i = za();
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
		return Ba.fromPartial(e ?? {});
	},
	fromPartial(e) {
		let t = za();
		return t.name = e.name ?? "", t.arguments = e.arguments ?? "", t;
	}
};
function Va() {
	return {
		value: "",
		mimeType: ""
	};
}
var Ha = {
	encode(e, t = new W()) {
		return e.value !== "" && t.uint32(10).string(e.value), e.mimeType !== "" && t.uint32(18).string(e.mimeType), t;
	},
	decode(e, t) {
		let n = e instanceof G ? e : new G(e), r = t === void 0 ? n.len : n.pos + t, i = Va();
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
		return t.value = e.value ?? "", t.mimeType = e.mimeType ?? "", t;
	}
};
function Ua() {
	return {
		value: "",
		mimeType: void 0
	};
}
var Wa = {
	encode(e, t = new W()) {
		return e.value !== "" && t.uint32(10).string(e.value), e.mimeType !== void 0 && t.uint32(18).string(e.mimeType), t;
	},
	decode(e, t) {
		let n = e instanceof G ? e : new G(e), r = t === void 0 ? n.len : n.pos + t, i = Ua();
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
		return Wa.fromPartial(e ?? {});
	},
	fromPartial(e) {
		let t = Ua();
		return t.value = e.value ?? "", t.mimeType = e.mimeType ?? void 0, t;
	}
};
function Ga() {
	return {
		data: void 0,
		url: void 0
	};
}
var q = {
	encode(e, t = new W()) {
		return e.data !== void 0 && Ha.encode(e.data, t.uint32(10).fork()).join(), e.url !== void 0 && Wa.encode(e.url, t.uint32(18).fork()).join(), t;
	},
	decode(e, t) {
		let n = e instanceof G ? e : new G(e), r = t === void 0 ? n.len : n.pos + t, i = Ga();
		for (; n.pos < r;) {
			let e = n.uint32();
			switch (e >>> 3) {
				case 1:
					if (e !== 10) break;
					i.data = Ha.decode(n, n.uint32());
					continue;
				case 2:
					if (e !== 18) break;
					i.url = Wa.decode(n, n.uint32());
					continue;
			}
			if ((e & 7) == 4 || e === 0) break;
			n.skip(e & 7);
		}
		return i;
	},
	create(e) {
		return q.fromPartial(e ?? {});
	},
	fromPartial(e) {
		let t = Ga();
		return t.data = e.data !== void 0 && e.data !== null ? Ha.fromPartial(e.data) : void 0, t.url = e.url !== void 0 && e.url !== null ? Wa.fromPartial(e.url) : void 0, t;
	}
};
function Ka() {
	return { text: "" };
}
var qa = {
	encode(e, t = new W()) {
		return e.text !== "" && t.uint32(10).string(e.text), t;
	},
	decode(e, t) {
		let n = e instanceof G ? e : new G(e), r = t === void 0 ? n.len : n.pos + t, i = Ka();
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
		return qa.fromPartial(e ?? {});
	},
	fromPartial(e) {
		let t = Ka();
		return t.text = e.text ?? "", t;
	}
};
function Ja() {
	return {
		source: void 0,
		metadata: void 0
	};
}
var Ya = {
	encode(e, t = new W()) {
		return e.source !== void 0 && q.encode(e.source, t.uint32(10).fork()).join(), e.metadata !== void 0 && K.encode(K.wrap(e.metadata), t.uint32(18).fork()).join(), t;
	},
	decode(e, t) {
		let n = e instanceof G ? e : new G(e), r = t === void 0 ? n.len : n.pos + t, i = Ja();
		for (; n.pos < r;) {
			let e = n.uint32();
			switch (e >>> 3) {
				case 1:
					if (e !== 10) break;
					i.source = q.decode(n, n.uint32());
					continue;
				case 2:
					if (e !== 18) break;
					i.metadata = K.unwrap(K.decode(n, n.uint32()));
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
		return t.source = e.source !== void 0 && e.source !== null ? q.fromPartial(e.source) : void 0, t.metadata = e.metadata ?? void 0, t;
	}
};
function Xa() {
	return {
		source: void 0,
		metadata: void 0
	};
}
var Za = {
	encode(e, t = new W()) {
		return e.source !== void 0 && q.encode(e.source, t.uint32(10).fork()).join(), e.metadata !== void 0 && K.encode(K.wrap(e.metadata), t.uint32(18).fork()).join(), t;
	},
	decode(e, t) {
		let n = e instanceof G ? e : new G(e), r = t === void 0 ? n.len : n.pos + t, i = Xa();
		for (; n.pos < r;) {
			let e = n.uint32();
			switch (e >>> 3) {
				case 1:
					if (e !== 10) break;
					i.source = q.decode(n, n.uint32());
					continue;
				case 2:
					if (e !== 18) break;
					i.metadata = K.unwrap(K.decode(n, n.uint32()));
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
		return t.source = e.source !== void 0 && e.source !== null ? q.fromPartial(e.source) : void 0, t.metadata = e.metadata ?? void 0, t;
	}
};
function Qa() {
	return {
		source: void 0,
		metadata: void 0
	};
}
var $a = {
	encode(e, t = new W()) {
		return e.source !== void 0 && q.encode(e.source, t.uint32(10).fork()).join(), e.metadata !== void 0 && K.encode(K.wrap(e.metadata), t.uint32(18).fork()).join(), t;
	},
	decode(e, t) {
		let n = e instanceof G ? e : new G(e), r = t === void 0 ? n.len : n.pos + t, i = Qa();
		for (; n.pos < r;) {
			let e = n.uint32();
			switch (e >>> 3) {
				case 1:
					if (e !== 10) break;
					i.source = q.decode(n, n.uint32());
					continue;
				case 2:
					if (e !== 18) break;
					i.metadata = K.unwrap(K.decode(n, n.uint32()));
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
		return t.source = e.source !== void 0 && e.source !== null ? q.fromPartial(e.source) : void 0, t.metadata = e.metadata ?? void 0, t;
	}
};
function eo() {
	return {
		source: void 0,
		metadata: void 0
	};
}
var to = {
	encode(e, t = new W()) {
		return e.source !== void 0 && q.encode(e.source, t.uint32(10).fork()).join(), e.metadata !== void 0 && K.encode(K.wrap(e.metadata), t.uint32(18).fork()).join(), t;
	},
	decode(e, t) {
		let n = e instanceof G ? e : new G(e), r = t === void 0 ? n.len : n.pos + t, i = eo();
		for (; n.pos < r;) {
			let e = n.uint32();
			switch (e >>> 3) {
				case 1:
					if (e !== 10) break;
					i.source = q.decode(n, n.uint32());
					continue;
				case 2:
					if (e !== 18) break;
					i.metadata = K.unwrap(K.decode(n, n.uint32()));
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
		return t.source = e.source !== void 0 && e.source !== null ? q.fromPartial(e.source) : void 0, t.metadata = e.metadata ?? void 0, t;
	}
};
function no() {
	return {
		text: void 0,
		image: void 0,
		audio: void 0,
		video: void 0,
		document: void 0
	};
}
var ro = {
	encode(e, t = new W()) {
		return e.text !== void 0 && qa.encode(e.text, t.uint32(10).fork()).join(), e.image !== void 0 && Ya.encode(e.image, t.uint32(18).fork()).join(), e.audio !== void 0 && Za.encode(e.audio, t.uint32(26).fork()).join(), e.video !== void 0 && $a.encode(e.video, t.uint32(34).fork()).join(), e.document !== void 0 && to.encode(e.document, t.uint32(42).fork()).join(), t;
	},
	decode(e, t) {
		let n = e instanceof G ? e : new G(e), r = t === void 0 ? n.len : n.pos + t, i = no();
		for (; n.pos < r;) {
			let e = n.uint32();
			switch (e >>> 3) {
				case 1:
					if (e !== 10) break;
					i.text = qa.decode(n, n.uint32());
					continue;
				case 2:
					if (e !== 18) break;
					i.image = Ya.decode(n, n.uint32());
					continue;
				case 3:
					if (e !== 26) break;
					i.audio = Za.decode(n, n.uint32());
					continue;
				case 4:
					if (e !== 34) break;
					i.video = $a.decode(n, n.uint32());
					continue;
				case 5:
					if (e !== 42) break;
					i.document = to.decode(n, n.uint32());
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
		return t.text = e.text !== void 0 && e.text !== null ? qa.fromPartial(e.text) : void 0, t.image = e.image !== void 0 && e.image !== null ? Ya.fromPartial(e.image) : void 0, t.audio = e.audio !== void 0 && e.audio !== null ? Za.fromPartial(e.audio) : void 0, t.video = e.video !== void 0 && e.video !== null ? $a.fromPartial(e.video) : void 0, t.document = e.document !== void 0 && e.document !== null ? to.fromPartial(e.document) : void 0, t;
	}
};
function io() {
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
var ao = {
	encode(e, t = new W()) {
		e.id !== "" && t.uint32(10).string(e.id), e.role !== "" && t.uint32(18).string(e.role), e.content !== void 0 && t.uint32(26).string(e.content), e.name !== void 0 && t.uint32(34).string(e.name);
		for (let n of e.toolCalls) Ra.encode(n, t.uint32(42).fork()).join();
		e.toolCallId !== void 0 && t.uint32(50).string(e.toolCallId), e.error !== void 0 && t.uint32(58).string(e.error);
		for (let n of e.contentParts) ro.encode(n, t.uint32(66).fork()).join();
		return t;
	},
	decode(e, t) {
		let n = e instanceof G ? e : new G(e), r = t === void 0 ? n.len : n.pos + t, i = io();
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
					i.toolCalls.push(Ra.decode(n, n.uint32()));
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
					i.contentParts.push(ro.decode(n, n.uint32()));
					continue;
			}
			if ((e & 7) == 4 || e === 0) break;
			n.skip(e & 7);
		}
		return i;
	},
	create(e) {
		return ao.fromPartial(e ?? {});
	},
	fromPartial(e) {
		let t = io();
		return t.id = e.id ?? "", t.role = e.role ?? "", t.content = e.content ?? void 0, t.name = e.name ?? void 0, t.toolCalls = e.toolCalls?.map((e) => Ra.fromPartial(e)) || [], t.toolCallId = e.toolCallId ?? void 0, t.error = e.error ?? void 0, t.contentParts = e.contentParts?.map((e) => ro.fromPartial(e)) || [], t;
	}
}, oo = /* @__PURE__ */ function(e) {
	return e[e.TEXT_MESSAGE_START = 0] = "TEXT_MESSAGE_START", e[e.TEXT_MESSAGE_CONTENT = 1] = "TEXT_MESSAGE_CONTENT", e[e.TEXT_MESSAGE_END = 2] = "TEXT_MESSAGE_END", e[e.TOOL_CALL_START = 3] = "TOOL_CALL_START", e[e.TOOL_CALL_ARGS = 4] = "TOOL_CALL_ARGS", e[e.TOOL_CALL_END = 5] = "TOOL_CALL_END", e[e.STATE_SNAPSHOT = 6] = "STATE_SNAPSHOT", e[e.STATE_DELTA = 7] = "STATE_DELTA", e[e.MESSAGES_SNAPSHOT = 8] = "MESSAGES_SNAPSHOT", e[e.RAW = 9] = "RAW", e[e.CUSTOM = 10] = "CUSTOM", e[e.RUN_STARTED = 11] = "RUN_STARTED", e[e.RUN_FINISHED = 12] = "RUN_FINISHED", e[e.RUN_ERROR = 13] = "RUN_ERROR", e[e.STEP_STARTED = 14] = "STEP_STARTED", e[e.STEP_FINISHED = 15] = "STEP_FINISHED", e[e.UNRECOGNIZED = -1] = "UNRECOGNIZED", e;
}({});
function so() {
	return {
		type: 0,
		timestamp: void 0,
		rawEvent: void 0
	};
}
var J = {
	encode(e, t = new W()) {
		return e.type !== 0 && t.uint32(8).int32(e.type), e.timestamp !== void 0 && t.uint32(16).int64(e.timestamp), e.rawEvent !== void 0 && K.encode(K.wrap(e.rawEvent), t.uint32(26).fork()).join(), t;
	},
	decode(e, t) {
		let n = e instanceof G ? e : new G(e), r = t === void 0 ? n.len : n.pos + t, i = so();
		for (; n.pos < r;) {
			let e = n.uint32();
			switch (e >>> 3) {
				case 1:
					if (e !== 8) break;
					i.type = n.int32();
					continue;
				case 2:
					if (e !== 16) break;
					i.timestamp = qo(n.int64());
					continue;
				case 3:
					if (e !== 26) break;
					i.rawEvent = K.unwrap(K.decode(n, n.uint32()));
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
		let t = so();
		return t.type = e.type ?? 0, t.timestamp = e.timestamp ?? void 0, t.rawEvent = e.rawEvent ?? void 0, t;
	}
};
function co() {
	return {
		baseEvent: void 0,
		messageId: "",
		role: void 0,
		name: void 0
	};
}
var lo = {
	encode(e, t = new W()) {
		return e.baseEvent !== void 0 && J.encode(e.baseEvent, t.uint32(10).fork()).join(), e.messageId !== "" && t.uint32(18).string(e.messageId), e.role !== void 0 && t.uint32(26).string(e.role), e.name !== void 0 && t.uint32(34).string(e.name), t;
	},
	decode(e, t) {
		let n = e instanceof G ? e : new G(e), r = t === void 0 ? n.len : n.pos + t, i = co();
		for (; n.pos < r;) {
			let e = n.uint32();
			switch (e >>> 3) {
				case 1:
					if (e !== 10) break;
					i.baseEvent = J.decode(n, n.uint32());
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
		return lo.fromPartial(e ?? {});
	},
	fromPartial(e) {
		let t = co();
		return t.baseEvent = e.baseEvent !== void 0 && e.baseEvent !== null ? J.fromPartial(e.baseEvent) : void 0, t.messageId = e.messageId ?? "", t.role = e.role ?? void 0, t.name = e.name ?? void 0, t;
	}
};
function uo() {
	return {
		baseEvent: void 0,
		messageId: "",
		delta: ""
	};
}
var fo = {
	encode(e, t = new W()) {
		return e.baseEvent !== void 0 && J.encode(e.baseEvent, t.uint32(10).fork()).join(), e.messageId !== "" && t.uint32(18).string(e.messageId), e.delta !== "" && t.uint32(26).string(e.delta), t;
	},
	decode(e, t) {
		let n = e instanceof G ? e : new G(e), r = t === void 0 ? n.len : n.pos + t, i = uo();
		for (; n.pos < r;) {
			let e = n.uint32();
			switch (e >>> 3) {
				case 1:
					if (e !== 10) break;
					i.baseEvent = J.decode(n, n.uint32());
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
		return fo.fromPartial(e ?? {});
	},
	fromPartial(e) {
		let t = uo();
		return t.baseEvent = e.baseEvent !== void 0 && e.baseEvent !== null ? J.fromPartial(e.baseEvent) : void 0, t.messageId = e.messageId ?? "", t.delta = e.delta ?? "", t;
	}
};
function po() {
	return {
		baseEvent: void 0,
		messageId: ""
	};
}
var mo = {
	encode(e, t = new W()) {
		return e.baseEvent !== void 0 && J.encode(e.baseEvent, t.uint32(10).fork()).join(), e.messageId !== "" && t.uint32(18).string(e.messageId), t;
	},
	decode(e, t) {
		let n = e instanceof G ? e : new G(e), r = t === void 0 ? n.len : n.pos + t, i = po();
		for (; n.pos < r;) {
			let e = n.uint32();
			switch (e >>> 3) {
				case 1:
					if (e !== 10) break;
					i.baseEvent = J.decode(n, n.uint32());
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
		return mo.fromPartial(e ?? {});
	},
	fromPartial(e) {
		let t = po();
		return t.baseEvent = e.baseEvent !== void 0 && e.baseEvent !== null ? J.fromPartial(e.baseEvent) : void 0, t.messageId = e.messageId ?? "", t;
	}
};
function ho() {
	return {
		baseEvent: void 0,
		toolCallId: "",
		toolCallName: "",
		parentMessageId: void 0
	};
}
var go = {
	encode(e, t = new W()) {
		return e.baseEvent !== void 0 && J.encode(e.baseEvent, t.uint32(10).fork()).join(), e.toolCallId !== "" && t.uint32(18).string(e.toolCallId), e.toolCallName !== "" && t.uint32(26).string(e.toolCallName), e.parentMessageId !== void 0 && t.uint32(34).string(e.parentMessageId), t;
	},
	decode(e, t) {
		let n = e instanceof G ? e : new G(e), r = t === void 0 ? n.len : n.pos + t, i = ho();
		for (; n.pos < r;) {
			let e = n.uint32();
			switch (e >>> 3) {
				case 1:
					if (e !== 10) break;
					i.baseEvent = J.decode(n, n.uint32());
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
		return go.fromPartial(e ?? {});
	},
	fromPartial(e) {
		let t = ho();
		return t.baseEvent = e.baseEvent !== void 0 && e.baseEvent !== null ? J.fromPartial(e.baseEvent) : void 0, t.toolCallId = e.toolCallId ?? "", t.toolCallName = e.toolCallName ?? "", t.parentMessageId = e.parentMessageId ?? void 0, t;
	}
};
function _o() {
	return {
		baseEvent: void 0,
		toolCallId: "",
		delta: ""
	};
}
var vo = {
	encode(e, t = new W()) {
		return e.baseEvent !== void 0 && J.encode(e.baseEvent, t.uint32(10).fork()).join(), e.toolCallId !== "" && t.uint32(18).string(e.toolCallId), e.delta !== "" && t.uint32(26).string(e.delta), t;
	},
	decode(e, t) {
		let n = e instanceof G ? e : new G(e), r = t === void 0 ? n.len : n.pos + t, i = _o();
		for (; n.pos < r;) {
			let e = n.uint32();
			switch (e >>> 3) {
				case 1:
					if (e !== 10) break;
					i.baseEvent = J.decode(n, n.uint32());
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
		return vo.fromPartial(e ?? {});
	},
	fromPartial(e) {
		let t = _o();
		return t.baseEvent = e.baseEvent !== void 0 && e.baseEvent !== null ? J.fromPartial(e.baseEvent) : void 0, t.toolCallId = e.toolCallId ?? "", t.delta = e.delta ?? "", t;
	}
};
function yo() {
	return {
		baseEvent: void 0,
		toolCallId: ""
	};
}
var bo = {
	encode(e, t = new W()) {
		return e.baseEvent !== void 0 && J.encode(e.baseEvent, t.uint32(10).fork()).join(), e.toolCallId !== "" && t.uint32(18).string(e.toolCallId), t;
	},
	decode(e, t) {
		let n = e instanceof G ? e : new G(e), r = t === void 0 ? n.len : n.pos + t, i = yo();
		for (; n.pos < r;) {
			let e = n.uint32();
			switch (e >>> 3) {
				case 1:
					if (e !== 10) break;
					i.baseEvent = J.decode(n, n.uint32());
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
		return bo.fromPartial(e ?? {});
	},
	fromPartial(e) {
		let t = yo();
		return t.baseEvent = e.baseEvent !== void 0 && e.baseEvent !== null ? J.fromPartial(e.baseEvent) : void 0, t.toolCallId = e.toolCallId ?? "", t;
	}
};
function xo() {
	return {
		baseEvent: void 0,
		snapshot: void 0
	};
}
var So = {
	encode(e, t = new W()) {
		return e.baseEvent !== void 0 && J.encode(e.baseEvent, t.uint32(10).fork()).join(), e.snapshot !== void 0 && K.encode(K.wrap(e.snapshot), t.uint32(18).fork()).join(), t;
	},
	decode(e, t) {
		let n = e instanceof G ? e : new G(e), r = t === void 0 ? n.len : n.pos + t, i = xo();
		for (; n.pos < r;) {
			let e = n.uint32();
			switch (e >>> 3) {
				case 1:
					if (e !== 10) break;
					i.baseEvent = J.decode(n, n.uint32());
					continue;
				case 2:
					if (e !== 18) break;
					i.snapshot = K.unwrap(K.decode(n, n.uint32()));
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
		return t.baseEvent = e.baseEvent !== void 0 && e.baseEvent !== null ? J.fromPartial(e.baseEvent) : void 0, t.snapshot = e.snapshot ?? void 0, t;
	}
};
function Co() {
	return {
		baseEvent: void 0,
		delta: []
	};
}
var wo = {
	encode(e, t = new W()) {
		e.baseEvent !== void 0 && J.encode(e.baseEvent, t.uint32(10).fork()).join();
		for (let n of e.delta) Ia.encode(n, t.uint32(18).fork()).join();
		return t;
	},
	decode(e, t) {
		let n = e instanceof G ? e : new G(e), r = t === void 0 ? n.len : n.pos + t, i = Co();
		for (; n.pos < r;) {
			let e = n.uint32();
			switch (e >>> 3) {
				case 1:
					if (e !== 10) break;
					i.baseEvent = J.decode(n, n.uint32());
					continue;
				case 2:
					if (e !== 18) break;
					i.delta.push(Ia.decode(n, n.uint32()));
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
		return t.baseEvent = e.baseEvent !== void 0 && e.baseEvent !== null ? J.fromPartial(e.baseEvent) : void 0, t.delta = e.delta?.map((e) => Ia.fromPartial(e)) || [], t;
	}
};
function To() {
	return {
		baseEvent: void 0,
		messages: []
	};
}
var Eo = {
	encode(e, t = new W()) {
		e.baseEvent !== void 0 && J.encode(e.baseEvent, t.uint32(10).fork()).join();
		for (let n of e.messages) ao.encode(n, t.uint32(18).fork()).join();
		return t;
	},
	decode(e, t) {
		let n = e instanceof G ? e : new G(e), r = t === void 0 ? n.len : n.pos + t, i = To();
		for (; n.pos < r;) {
			let e = n.uint32();
			switch (e >>> 3) {
				case 1:
					if (e !== 10) break;
					i.baseEvent = J.decode(n, n.uint32());
					continue;
				case 2:
					if (e !== 18) break;
					i.messages.push(ao.decode(n, n.uint32()));
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
		return t.baseEvent = e.baseEvent !== void 0 && e.baseEvent !== null ? J.fromPartial(e.baseEvent) : void 0, t.messages = e.messages?.map((e) => ao.fromPartial(e)) || [], t;
	}
};
function Do() {
	return {
		baseEvent: void 0,
		event: void 0,
		source: void 0
	};
}
var Oo = {
	encode(e, t = new W()) {
		return e.baseEvent !== void 0 && J.encode(e.baseEvent, t.uint32(10).fork()).join(), e.event !== void 0 && K.encode(K.wrap(e.event), t.uint32(18).fork()).join(), e.source !== void 0 && t.uint32(26).string(e.source), t;
	},
	decode(e, t) {
		let n = e instanceof G ? e : new G(e), r = t === void 0 ? n.len : n.pos + t, i = Do();
		for (; n.pos < r;) {
			let e = n.uint32();
			switch (e >>> 3) {
				case 1:
					if (e !== 10) break;
					i.baseEvent = J.decode(n, n.uint32());
					continue;
				case 2:
					if (e !== 18) break;
					i.event = K.unwrap(K.decode(n, n.uint32()));
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
		return Oo.fromPartial(e ?? {});
	},
	fromPartial(e) {
		let t = Do();
		return t.baseEvent = e.baseEvent !== void 0 && e.baseEvent !== null ? J.fromPartial(e.baseEvent) : void 0, t.event = e.event ?? void 0, t.source = e.source ?? void 0, t;
	}
};
function ko() {
	return {
		baseEvent: void 0,
		name: "",
		value: void 0
	};
}
var Ao = {
	encode(e, t = new W()) {
		return e.baseEvent !== void 0 && J.encode(e.baseEvent, t.uint32(10).fork()).join(), e.name !== "" && t.uint32(18).string(e.name), e.value !== void 0 && K.encode(K.wrap(e.value), t.uint32(26).fork()).join(), t;
	},
	decode(e, t) {
		let n = e instanceof G ? e : new G(e), r = t === void 0 ? n.len : n.pos + t, i = ko();
		for (; n.pos < r;) {
			let e = n.uint32();
			switch (e >>> 3) {
				case 1:
					if (e !== 10) break;
					i.baseEvent = J.decode(n, n.uint32());
					continue;
				case 2:
					if (e !== 18) break;
					i.name = n.string();
					continue;
				case 3:
					if (e !== 26) break;
					i.value = K.unwrap(K.decode(n, n.uint32()));
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
		return t.baseEvent = e.baseEvent !== void 0 && e.baseEvent !== null ? J.fromPartial(e.baseEvent) : void 0, t.name = e.name ?? "", t.value = e.value ?? void 0, t;
	}
};
function jo() {
	return {
		baseEvent: void 0,
		threadId: "",
		runId: ""
	};
}
var Mo = {
	encode(e, t = new W()) {
		return e.baseEvent !== void 0 && J.encode(e.baseEvent, t.uint32(10).fork()).join(), e.threadId !== "" && t.uint32(18).string(e.threadId), e.runId !== "" && t.uint32(26).string(e.runId), t;
	},
	decode(e, t) {
		let n = e instanceof G ? e : new G(e), r = t === void 0 ? n.len : n.pos + t, i = jo();
		for (; n.pos < r;) {
			let e = n.uint32();
			switch (e >>> 3) {
				case 1:
					if (e !== 10) break;
					i.baseEvent = J.decode(n, n.uint32());
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
		return Mo.fromPartial(e ?? {});
	},
	fromPartial(e) {
		let t = jo();
		return t.baseEvent = e.baseEvent !== void 0 && e.baseEvent !== null ? J.fromPartial(e.baseEvent) : void 0, t.threadId = e.threadId ?? "", t.runId = e.runId ?? "", t;
	}
};
function No() {
	return {
		baseEvent: void 0,
		threadId: "",
		runId: "",
		result: void 0
	};
}
var Po = {
	encode(e, t = new W()) {
		return e.baseEvent !== void 0 && J.encode(e.baseEvent, t.uint32(10).fork()).join(), e.threadId !== "" && t.uint32(18).string(e.threadId), e.runId !== "" && t.uint32(26).string(e.runId), e.result !== void 0 && K.encode(K.wrap(e.result), t.uint32(34).fork()).join(), t;
	},
	decode(e, t) {
		let n = e instanceof G ? e : new G(e), r = t === void 0 ? n.len : n.pos + t, i = No();
		for (; n.pos < r;) {
			let e = n.uint32();
			switch (e >>> 3) {
				case 1:
					if (e !== 10) break;
					i.baseEvent = J.decode(n, n.uint32());
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
					i.result = K.unwrap(K.decode(n, n.uint32()));
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
		return t.baseEvent = e.baseEvent !== void 0 && e.baseEvent !== null ? J.fromPartial(e.baseEvent) : void 0, t.threadId = e.threadId ?? "", t.runId = e.runId ?? "", t.result = e.result ?? void 0, t;
	}
};
function Fo() {
	return {
		baseEvent: void 0,
		code: void 0,
		message: ""
	};
}
var Io = {
	encode(e, t = new W()) {
		return e.baseEvent !== void 0 && J.encode(e.baseEvent, t.uint32(10).fork()).join(), e.code !== void 0 && t.uint32(18).string(e.code), e.message !== "" && t.uint32(26).string(e.message), t;
	},
	decode(e, t) {
		let n = e instanceof G ? e : new G(e), r = t === void 0 ? n.len : n.pos + t, i = Fo();
		for (; n.pos < r;) {
			let e = n.uint32();
			switch (e >>> 3) {
				case 1:
					if (e !== 10) break;
					i.baseEvent = J.decode(n, n.uint32());
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
		return Io.fromPartial(e ?? {});
	},
	fromPartial(e) {
		let t = Fo();
		return t.baseEvent = e.baseEvent !== void 0 && e.baseEvent !== null ? J.fromPartial(e.baseEvent) : void 0, t.code = e.code ?? void 0, t.message = e.message ?? "", t;
	}
};
function Lo() {
	return {
		baseEvent: void 0,
		stepName: ""
	};
}
var Ro = {
	encode(e, t = new W()) {
		return e.baseEvent !== void 0 && J.encode(e.baseEvent, t.uint32(10).fork()).join(), e.stepName !== "" && t.uint32(18).string(e.stepName), t;
	},
	decode(e, t) {
		let n = e instanceof G ? e : new G(e), r = t === void 0 ? n.len : n.pos + t, i = Lo();
		for (; n.pos < r;) {
			let e = n.uint32();
			switch (e >>> 3) {
				case 1:
					if (e !== 10) break;
					i.baseEvent = J.decode(n, n.uint32());
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
		return t.baseEvent = e.baseEvent !== void 0 && e.baseEvent !== null ? J.fromPartial(e.baseEvent) : void 0, t.stepName = e.stepName ?? "", t;
	}
};
function zo() {
	return {
		baseEvent: void 0,
		stepName: ""
	};
}
var Bo = {
	encode(e, t = new W()) {
		return e.baseEvent !== void 0 && J.encode(e.baseEvent, t.uint32(10).fork()).join(), e.stepName !== "" && t.uint32(18).string(e.stepName), t;
	},
	decode(e, t) {
		let n = e instanceof G ? e : new G(e), r = t === void 0 ? n.len : n.pos + t, i = zo();
		for (; n.pos < r;) {
			let e = n.uint32();
			switch (e >>> 3) {
				case 1:
					if (e !== 10) break;
					i.baseEvent = J.decode(n, n.uint32());
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
		return Bo.fromPartial(e ?? {});
	},
	fromPartial(e) {
		let t = zo();
		return t.baseEvent = e.baseEvent !== void 0 && e.baseEvent !== null ? J.fromPartial(e.baseEvent) : void 0, t.stepName = e.stepName ?? "", t;
	}
};
function Vo() {
	return {
		baseEvent: void 0,
		messageId: void 0,
		role: void 0,
		delta: void 0,
		name: void 0
	};
}
var Ho = {
	encode(e, t = new W()) {
		return e.baseEvent !== void 0 && J.encode(e.baseEvent, t.uint32(10).fork()).join(), e.messageId !== void 0 && t.uint32(18).string(e.messageId), e.role !== void 0 && t.uint32(26).string(e.role), e.delta !== void 0 && t.uint32(34).string(e.delta), e.name !== void 0 && t.uint32(42).string(e.name), t;
	},
	decode(e, t) {
		let n = e instanceof G ? e : new G(e), r = t === void 0 ? n.len : n.pos + t, i = Vo();
		for (; n.pos < r;) {
			let e = n.uint32();
			switch (e >>> 3) {
				case 1:
					if (e !== 10) break;
					i.baseEvent = J.decode(n, n.uint32());
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
		return Ho.fromPartial(e ?? {});
	},
	fromPartial(e) {
		let t = Vo();
		return t.baseEvent = e.baseEvent !== void 0 && e.baseEvent !== null ? J.fromPartial(e.baseEvent) : void 0, t.messageId = e.messageId ?? void 0, t.role = e.role ?? void 0, t.delta = e.delta ?? void 0, t.name = e.name ?? void 0, t;
	}
};
function Uo() {
	return {
		baseEvent: void 0,
		toolCallId: void 0,
		toolCallName: void 0,
		parentMessageId: void 0,
		delta: void 0
	};
}
var Wo = {
	encode(e, t = new W()) {
		return e.baseEvent !== void 0 && J.encode(e.baseEvent, t.uint32(10).fork()).join(), e.toolCallId !== void 0 && t.uint32(18).string(e.toolCallId), e.toolCallName !== void 0 && t.uint32(26).string(e.toolCallName), e.parentMessageId !== void 0 && t.uint32(34).string(e.parentMessageId), e.delta !== void 0 && t.uint32(42).string(e.delta), t;
	},
	decode(e, t) {
		let n = e instanceof G ? e : new G(e), r = t === void 0 ? n.len : n.pos + t, i = Uo();
		for (; n.pos < r;) {
			let e = n.uint32();
			switch (e >>> 3) {
				case 1:
					if (e !== 10) break;
					i.baseEvent = J.decode(n, n.uint32());
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
		return Wo.fromPartial(e ?? {});
	},
	fromPartial(e) {
		let t = Uo();
		return t.baseEvent = e.baseEvent !== void 0 && e.baseEvent !== null ? J.fromPartial(e.baseEvent) : void 0, t.toolCallId = e.toolCallId ?? void 0, t.toolCallName = e.toolCallName ?? void 0, t.parentMessageId = e.parentMessageId ?? void 0, t.delta = e.delta ?? void 0, t;
	}
};
function Go() {
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
var Ko = {
	encode(e, t = new W()) {
		return e.textMessageStart !== void 0 && lo.encode(e.textMessageStart, t.uint32(10).fork()).join(), e.textMessageContent !== void 0 && fo.encode(e.textMessageContent, t.uint32(18).fork()).join(), e.textMessageEnd !== void 0 && mo.encode(e.textMessageEnd, t.uint32(26).fork()).join(), e.toolCallStart !== void 0 && go.encode(e.toolCallStart, t.uint32(34).fork()).join(), e.toolCallArgs !== void 0 && vo.encode(e.toolCallArgs, t.uint32(42).fork()).join(), e.toolCallEnd !== void 0 && bo.encode(e.toolCallEnd, t.uint32(50).fork()).join(), e.stateSnapshot !== void 0 && So.encode(e.stateSnapshot, t.uint32(58).fork()).join(), e.stateDelta !== void 0 && wo.encode(e.stateDelta, t.uint32(66).fork()).join(), e.messagesSnapshot !== void 0 && Eo.encode(e.messagesSnapshot, t.uint32(74).fork()).join(), e.raw !== void 0 && Oo.encode(e.raw, t.uint32(82).fork()).join(), e.custom !== void 0 && Ao.encode(e.custom, t.uint32(90).fork()).join(), e.runStarted !== void 0 && Mo.encode(e.runStarted, t.uint32(98).fork()).join(), e.runFinished !== void 0 && Po.encode(e.runFinished, t.uint32(106).fork()).join(), e.runError !== void 0 && Io.encode(e.runError, t.uint32(114).fork()).join(), e.stepStarted !== void 0 && Ro.encode(e.stepStarted, t.uint32(122).fork()).join(), e.stepFinished !== void 0 && Bo.encode(e.stepFinished, t.uint32(130).fork()).join(), e.textMessageChunk !== void 0 && Ho.encode(e.textMessageChunk, t.uint32(138).fork()).join(), e.toolCallChunk !== void 0 && Wo.encode(e.toolCallChunk, t.uint32(146).fork()).join(), t;
	},
	decode(e, t) {
		let n = e instanceof G ? e : new G(e), r = t === void 0 ? n.len : n.pos + t, i = Go();
		for (; n.pos < r;) {
			let e = n.uint32();
			switch (e >>> 3) {
				case 1:
					if (e !== 10) break;
					i.textMessageStart = lo.decode(n, n.uint32());
					continue;
				case 2:
					if (e !== 18) break;
					i.textMessageContent = fo.decode(n, n.uint32());
					continue;
				case 3:
					if (e !== 26) break;
					i.textMessageEnd = mo.decode(n, n.uint32());
					continue;
				case 4:
					if (e !== 34) break;
					i.toolCallStart = go.decode(n, n.uint32());
					continue;
				case 5:
					if (e !== 42) break;
					i.toolCallArgs = vo.decode(n, n.uint32());
					continue;
				case 6:
					if (e !== 50) break;
					i.toolCallEnd = bo.decode(n, n.uint32());
					continue;
				case 7:
					if (e !== 58) break;
					i.stateSnapshot = So.decode(n, n.uint32());
					continue;
				case 8:
					if (e !== 66) break;
					i.stateDelta = wo.decode(n, n.uint32());
					continue;
				case 9:
					if (e !== 74) break;
					i.messagesSnapshot = Eo.decode(n, n.uint32());
					continue;
				case 10:
					if (e !== 82) break;
					i.raw = Oo.decode(n, n.uint32());
					continue;
				case 11:
					if (e !== 90) break;
					i.custom = Ao.decode(n, n.uint32());
					continue;
				case 12:
					if (e !== 98) break;
					i.runStarted = Mo.decode(n, n.uint32());
					continue;
				case 13:
					if (e !== 106) break;
					i.runFinished = Po.decode(n, n.uint32());
					continue;
				case 14:
					if (e !== 114) break;
					i.runError = Io.decode(n, n.uint32());
					continue;
				case 15:
					if (e !== 122) break;
					i.stepStarted = Ro.decode(n, n.uint32());
					continue;
				case 16:
					if (e !== 130) break;
					i.stepFinished = Bo.decode(n, n.uint32());
					continue;
				case 17:
					if (e !== 138) break;
					i.textMessageChunk = Ho.decode(n, n.uint32());
					continue;
				case 18:
					if (e !== 146) break;
					i.toolCallChunk = Wo.decode(n, n.uint32());
					continue;
			}
			if ((e & 7) == 4 || e === 0) break;
			n.skip(e & 7);
		}
		return i;
	},
	create(e) {
		return Ko.fromPartial(e ?? {});
	},
	fromPartial(e) {
		let t = Go();
		return t.textMessageStart = e.textMessageStart !== void 0 && e.textMessageStart !== null ? lo.fromPartial(e.textMessageStart) : void 0, t.textMessageContent = e.textMessageContent !== void 0 && e.textMessageContent !== null ? fo.fromPartial(e.textMessageContent) : void 0, t.textMessageEnd = e.textMessageEnd !== void 0 && e.textMessageEnd !== null ? mo.fromPartial(e.textMessageEnd) : void 0, t.toolCallStart = e.toolCallStart !== void 0 && e.toolCallStart !== null ? go.fromPartial(e.toolCallStart) : void 0, t.toolCallArgs = e.toolCallArgs !== void 0 && e.toolCallArgs !== null ? vo.fromPartial(e.toolCallArgs) : void 0, t.toolCallEnd = e.toolCallEnd !== void 0 && e.toolCallEnd !== null ? bo.fromPartial(e.toolCallEnd) : void 0, t.stateSnapshot = e.stateSnapshot !== void 0 && e.stateSnapshot !== null ? So.fromPartial(e.stateSnapshot) : void 0, t.stateDelta = e.stateDelta !== void 0 && e.stateDelta !== null ? wo.fromPartial(e.stateDelta) : void 0, t.messagesSnapshot = e.messagesSnapshot !== void 0 && e.messagesSnapshot !== null ? Eo.fromPartial(e.messagesSnapshot) : void 0, t.raw = e.raw !== void 0 && e.raw !== null ? Oo.fromPartial(e.raw) : void 0, t.custom = e.custom !== void 0 && e.custom !== null ? Ao.fromPartial(e.custom) : void 0, t.runStarted = e.runStarted !== void 0 && e.runStarted !== null ? Mo.fromPartial(e.runStarted) : void 0, t.runFinished = e.runFinished !== void 0 && e.runFinished !== null ? Po.fromPartial(e.runFinished) : void 0, t.runError = e.runError !== void 0 && e.runError !== null ? Io.fromPartial(e.runError) : void 0, t.stepStarted = e.stepStarted !== void 0 && e.stepStarted !== null ? Ro.fromPartial(e.stepStarted) : void 0, t.stepFinished = e.stepFinished !== void 0 && e.stepFinished !== null ? Bo.fromPartial(e.stepFinished) : void 0, t.textMessageChunk = e.textMessageChunk !== void 0 && e.textMessageChunk !== null ? Ho.fromPartial(e.textMessageChunk) : void 0, t.toolCallChunk = e.toolCallChunk !== void 0 && e.toolCallChunk !== null ? Wo.fromPartial(e.toolCallChunk) : void 0, t;
	}
};
function qo(e) {
	let t = globalThis.Number(e.toString());
	if (t > globalThis.Number.MAX_SAFE_INTEGER) throw new globalThis.Error("Value is larger than Number.MAX_SAFE_INTEGER");
	if (t < globalThis.Number.MIN_SAFE_INTEGER) throw new globalThis.Error("Value is smaller than Number.MIN_SAFE_INTEGER");
	return t;
}
var Jo = (e) => {
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
}, Yo = (e) => {
	if (!(!e || typeof e != "object")) {
		if (e.text) return {
			type: "text",
			text: e.text.text
		};
		if (e.image) return {
			type: "image",
			source: Jo(e.image.source),
			metadata: e.image.metadata
		};
		if (e.audio) return {
			type: "audio",
			source: Jo(e.audio.source),
			metadata: e.audio.metadata
		};
		if (e.video) return {
			type: "video",
			source: Jo(e.video.source),
			metadata: e.video.metadata
		};
		if (e.document) return {
			type: "document",
			source: Jo(e.document.source),
			metadata: e.document.metadata
		};
	}
};
function Xo(e) {
	let t = Ko.decode(e), n = Object.values(t).find((e) => e !== void 0);
	if (!n) throw Error("Invalid event");
	if (n.type = oo[n.baseEvent.type], n.timestamp = n.baseEvent.timestamp, n.rawEvent = n.baseEvent.rawEvent, n.type === N.MESSAGES_SNAPSHOT) for (let e of n.messages) {
		let t = e;
		if (t.role === "user" && Array.isArray(t.contentParts)) {
			let e = t.contentParts.map((e) => Yo(e)).filter((e) => e !== void 0);
			e.length > 0 && (t.content = e);
		}
		Array.isArray(t.contentParts) && t.contentParts.length === 0 && (t.contentParts = void 0), t.toolCalls?.length === 0 && (t.toolCalls = void 0);
	}
	if (n.type === N.STATE_DELTA) for (let e of n.delta) e.op = Pa[e.op].toLowerCase(), Object.keys(e).forEach((t) => {
		e[t] === void 0 && delete e[t];
	});
	return Object.keys(n).forEach((e) => {
		n[e] === void 0 && delete n[e];
	}), Mn.parse(n);
}
//#endregion
//#region node_modules/compare-versions/lib/esm/utils.js
var Zo = /^[v^~<>=]*?(\d+)(?:\.([x*]|\d+)(?:\.([x*]|\d+)(?:\.([x*]|\d+))?(?:-([\da-z\-]+(?:\.[\da-z\-]+)*))?(?:\+[\da-z\-]+(?:\.[\da-z\-]+)*)?)?)?$/i, Qo = (e) => {
	if (typeof e != "string") throw TypeError("Invalid argument expected string");
	let t = e.match(Zo);
	if (!t) throw Error(`Invalid argument not valid semver ('${e}' received)`);
	return t.shift(), t;
}, $o = (e) => e === "*" || e === "x" || e === "X", es = (e) => {
	let t = parseInt(e, 10);
	return isNaN(t) ? e : t;
}, ts = (e, t) => typeof e == typeof t ? [e, t] : [String(e), String(t)], ns = (e, t) => {
	if ($o(e) || $o(t)) return 0;
	let [n, r] = ts(es(e), es(t));
	return n > r ? 1 : n < r ? -1 : 0;
}, rs = (e, t) => {
	for (let n = 0; n < Math.max(e.length, t.length); n++) {
		let r = ns(e[n] || "0", t[n] || "0");
		if (r !== 0) return r;
	}
	return 0;
}, is = (e, t) => {
	let n = Qo(e), r = Qo(t), i = n.pop(), a = r.pop(), o = rs(n, r);
	return o === 0 ? i && a ? rs(i.split("."), a.split(".")) : i || a ? i ? -1 : 1 : 0 : o;
}, Y = (e) => {
	if (typeof structuredClone == "function") return structuredClone(e);
	try {
		return JSON.parse(JSON.stringify(e));
	} catch {
		return Array.isArray(e) ? [...e] : { ...e };
	}
};
function as() {
	return c();
}
function os(e) {
	if (Object.freeze(e), typeof e == "object" && e) for (let t of Object.values(e)) typeof t == "object" && t && !Object.isFrozen(t) && os(t);
	return e;
}
async function X(e, t, n, r) {
	let i = typeof process < "u" && process.env !== void 0, a = i && (process.env.NODE_ENV === "test" || !!process.env.VITEST_WORKER_ID), o = i && (process.env.NODE_ENV === "development" || process.env.NODE_ENV === "test" || !!process.env.VITEST_WORKER_ID), s = Y(t), c = Y(n), l = s, u = c, d;
	for (let t of e) try {
		o && (os(l), os(u));
		let e = await r(t, l, u);
		if (e === void 0) continue;
		if (e.messages !== void 0 && e.messages !== l && (l = Y(e.messages)), e.state !== void 0 && e.state !== u && (u = Y(e.state)), d = e.stopPropagation, d === !0) break;
	} catch (e) {
		if (o && e instanceof TypeError) {
			if (a) throw e;
			console.error("AG-UI: Subscriber attempted to mutate frozen inputs in-place. Return mutations via AgentStateMutation instead of mutating directly.", e);
		} else a || console.error("Subscriber error:", e);
		continue;
	}
	return {
		...l === s ? {} : { messages: o && Object.isFrozen(l) ? Y(l) : l },
		...u === c ? {} : { state: o && Object.isFrozen(u) ? Y(u) : u },
		...d === void 0 ? {} : { stopPropagation: d }
	};
}
function ss(e) {
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
function cs(e) {
	if (e instanceof ls) return e;
	if (e === !0) return new ls(ss(!0));
}
var ls = class {
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
function us(e) {
	return e.enabled ? new ls(e) : void 0;
}
function ds(e, t, n) {
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
var fs = (e, t, n, r, i) => {
	let a = cs(i), o = Y(n.messages), s = Y(e.state), c = {}, l = (e) => {
		e.messages !== void 0 && (o = e.messages, c.messages = e.messages), e.state !== void 0 && (s = e.state, c.state = e.state);
	}, u = () => {
		let e = Y(c);
		return c = {}, e.messages !== void 0 || e.state !== void 0 ? V(e) : ui;
	};
	return t.pipe(Zi(async (t) => {
		let i = await X(r, o, s, (r, i, a) => r.onEvent?.({
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
			case N.TEXT_MESSAGE_START: {
				let i = await X(r, o, s, (r, i, a) => r.onTextMessageStartEvent?.({
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
			case N.TEXT_MESSAGE_CONTENT: {
				let { messageId: i, delta: a } = t, c = o.find((e) => e.id === i);
				if (!c) return console.warn(`TEXT_MESSAGE_CONTENT: No message found with ID '${i}'`), u();
				let d = await X(r, o, s, (r, i, a) => r.onTextMessageContentEvent?.({
					event: t,
					messages: i,
					state: a,
					agent: n,
					input: e,
					textMessageBuffer: typeof c.content == "string" ? c.content : ""
				}));
				return l(d), d.stopPropagation !== !0 && (c.content = `${typeof c.content == "string" ? c.content : ""}${a}`, l({ messages: o })), u();
			}
			case N.TEXT_MESSAGE_END: {
				let { messageId: i } = t, a = o.find((e) => e.id === i);
				return a ? (l(await X(r, o, s, (r, i, o) => r.onTextMessageEndEvent?.({
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
			case N.TOOL_CALL_START: {
				let i = await X(r, o, s, (r, i, a) => r.onToolCallStartEvent?.({
					event: t,
					messages: i,
					state: a,
					agent: n,
					input: e
				}));
				if (l(i), i.stopPropagation !== !0) {
					let { toolCallId: e, toolCallName: n, parentMessageId: r } = t, i = ds(o, r, e);
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
			case N.TOOL_CALL_ARGS: {
				let { toolCallId: i, delta: a } = t, c = o.find((e) => e.toolCalls?.some((e) => e.id === i));
				if (!c) return console.warn(`TOOL_CALL_ARGS: No message found containing tool call with ID '${i}'`), u();
				let d = c.toolCalls?.find((e) => e.id === i);
				if (!d) return console.warn(`TOOL_CALL_ARGS: No tool call found with ID '${i}'`), u();
				let f = await X(r, o, s, (r, i, a) => {
					let o = d.function.arguments, s = d.function.name, c = {};
					try {
						c = ia(o);
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
			case N.TOOL_CALL_END: {
				let { toolCallId: i } = t, a = o.find((e) => e.toolCalls?.some((e) => e.id === i));
				if (!a) return console.warn(`TOOL_CALL_END: No message found containing tool call with ID '${i}'`), u();
				let c = a.toolCalls?.find((e) => e.id === i);
				return c ? (l(await X(r, o, s, (r, i, a) => {
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
			case N.TOOL_CALL_RESULT: {
				let i = await X(r, o, s, (r, i, a) => r.onToolCallResultEvent?.({
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
			case N.STATE_SNAPSHOT: {
				let i = await X(r, o, s, (r, i, a) => r.onStateSnapshotEvent?.({
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
			case N.STATE_DELTA: {
				let i = await X(r, o, s, (r, i, a) => r.onStateDeltaEvent?.({
					event: t,
					messages: i,
					state: a,
					agent: n,
					input: e
				}));
				if (l(i), i.stopPropagation !== !0) {
					let { delta: e } = t;
					try {
						s = fr.applyPatch(s, e, !0, !1).newDocument, l({ state: s });
					} catch (t) {
						let n = t instanceof Error ? t.message : String(t);
						console.warn(`Failed to apply state patch:\nCurrent state: ${JSON.stringify(s, null, 2)}\nPatch operations: ${JSON.stringify(e, null, 2)}\nError: ${n}`);
					}
				}
				return u();
			}
			case N.MESSAGES_SNAPSHOT: {
				let i = await X(r, o, s, (r, i, a) => r.onMessagesSnapshotEvent?.({
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
			case N.ACTIVITY_SNAPSHOT: {
				let i = t, a = o.findIndex((e) => e.id === i.messageId), c = a >= 0 ? o[a] : void 0, d = c?.role === "activity" ? c : void 0, f = i.replace ?? !0, p = await X(r, o, s, (t, r, a) => t.onActivitySnapshotEvent?.({
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
						content: Y(i.content)
					}, c;
					a === -1 ? (o.push(t), c = t) : d ? f && (o[a] = {
						...d,
						activityType: i.activityType,
						content: Y(i.content)
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
			case N.ACTIVITY_DELTA: {
				let i = t, a = o.findIndex((e) => e.id === i.messageId);
				if (a === -1) return u();
				let c = o[a];
				if (c.role !== "activity") return console.warn(`ACTIVITY_DELTA: Message '${i.messageId}' is not an activity message`), u();
				let d = c, f = await X(r, o, s, (t, r, a) => t.onActivityDeltaEvent?.({
					event: i,
					messages: r,
					state: a,
					agent: n,
					input: e,
					activityMessage: d
				}));
				if (l(f), f.stopPropagation !== !0) try {
					let e = Y(d.content ?? {}), t = fr.applyPatch(e, i.patch ?? [], !0, !1).newDocument;
					o[a] = {
						...d,
						content: Y(t),
						activityType: i.activityType
					}, l({ messages: o });
				} catch (e) {
					let t = e instanceof Error ? e.message : String(e);
					console.warn(`Failed to apply activity patch for '${i.messageId}': ${t}`);
				}
				return u();
			}
			case N.RAW: return l(await X(r, o, s, (r, i, a) => r.onRawEvent?.({
				event: t,
				messages: i,
				state: a,
				agent: n,
				input: e
			}))), u();
			case N.CUSTOM: return l(await X(r, o, s, (r, i, a) => r.onCustomEvent?.({
				event: t,
				messages: i,
				state: a,
				agent: n,
				input: e
			}))), u();
			case N.RUN_STARTED: {
				let i = await X(r, o, s, (r, i, a) => r.onRunStartedEvent?.({
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
			case N.RUN_FINISHED: return l(await X(r, o, s, (r, i, a) => r.onRunFinishedEvent?.({
				event: t,
				messages: i,
				state: a,
				agent: n,
				input: e,
				result: t.result
			}))), u();
			case N.RUN_ERROR: return l(await X(r, o, s, (r, i, a) => r.onRunErrorEvent?.({
				event: t,
				messages: i,
				state: a,
				agent: n,
				input: e
			}))), u();
			case N.STEP_STARTED: return l(await X(r, o, s, (r, i, a) => r.onStepStartedEvent?.({
				event: t,
				messages: i,
				state: a,
				agent: n,
				input: e
			}))), u();
			case N.STEP_FINISHED: return l(await X(r, o, s, (r, i, a) => r.onStepFinishedEvent?.({
				event: t,
				messages: i,
				state: a,
				agent: n,
				input: e
			}))), u();
			case N.TEXT_MESSAGE_CHUNK: throw Error("TEXT_MESSAGE_CHUNK must be tranformed before being applied");
			case N.TOOL_CALL_CHUNK: throw Error("TOOL_CALL_CHUNK must be tranformed before being applied");
			case N.THINKING_START: return u();
			case N.THINKING_END: return u();
			case N.THINKING_TEXT_MESSAGE_START: return u();
			case N.THINKING_TEXT_MESSAGE_CONTENT: return u();
			case N.THINKING_TEXT_MESSAGE_END: return u();
			case N.REASONING_START: return l(await X(r, o, s, (r, i, a) => r.onReasoningStartEvent?.({
				event: t,
				messages: i,
				state: a,
				agent: n,
				input: e
			}))), u();
			case N.REASONING_MESSAGE_START: {
				let i = await X(r, o, s, (r, i, a) => r.onReasoningMessageStartEvent?.({
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
			case N.REASONING_MESSAGE_CONTENT: {
				let { messageId: i, delta: a } = t, c = o.find((e) => e.id === i);
				if (!c) return console.warn(`REASONING_MESSAGE_CONTENT: No message found with ID '${i}'`), u();
				let d = await X(r, o, s, (r, i, a) => r.onReasoningMessageContentEvent?.({
					event: t,
					messages: i,
					state: a,
					agent: n,
					input: e,
					reasoningMessageBuffer: typeof c.content == "string" ? c.content : ""
				}));
				return l(d), d.stopPropagation !== !0 && (c.content = `${typeof c.content == "string" ? c.content : ""}${a}`, l({ messages: o })), u();
			}
			case N.REASONING_MESSAGE_END: {
				let { messageId: i } = t, a = o.find((e) => e.id === i);
				return a ? (l(await X(r, o, s, (r, i, o) => r.onReasoningMessageEndEvent?.({
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
			case N.REASONING_MESSAGE_CHUNK: throw Error("REASONING_MESSAGE_CHUNK must be transformed before being applied");
			case N.REASONING_END: return l(await X(r, o, s, (r, i, a) => r.onReasoningEndEvent?.({
				event: t,
				messages: i,
				state: a,
				agent: n,
				input: e
			}))), u();
			case N.REASONING_ENCRYPTED_VALUE: {
				let { subtype: i, entityId: a, encryptedValue: d } = t, f = await X(r, o, s, (r, i, a) => r.onReasoningEncryptedValueEvent?.({
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
	}), Ji(), r.length > 0 ? Qi({}) : (e) => e);
}, ps = (e) => (t) => {
	let n = cs(e), r = /* @__PURE__ */ new Map(), i = /* @__PURE__ */ new Map(), a = !1, o = !1, s = !1, c = /* @__PURE__ */ new Map(), l = !1, u = !1, d = !1, f = () => {
		r.clear(), i.clear(), c.clear(), l = !1, u = !1, a = !1, o = !1, d = !0;
	};
	return t.pipe(qi((e) => {
		let t = e.type;
		if (n?.event("VERIFY", "Event:", e, { type: e.type }), o) return H(() => new M(`Cannot send event type '${t}': The run has already errored with 'RUN_ERROR'. No further events can be sent.`));
		if (a && t !== N.RUN_ERROR && t !== N.RUN_STARTED) return H(() => new M(`Cannot send event type '${t}': The run has already finished with 'RUN_FINISHED'. Start a new run with 'RUN_STARTED'.`));
		if (!s) {
			if (s = !0, t !== N.RUN_STARTED && t !== N.RUN_ERROR) return H(() => new M("First event must be 'RUN_STARTED'"));
		} else if (t === N.RUN_STARTED) {
			if (d && !a) return H(() => new M("Cannot send 'RUN_STARTED' while a run is still active. The previous run must be finished with 'RUN_FINISHED' before starting a new run."));
			a && f();
		}
		switch (t) {
			case N.TEXT_MESSAGE_START: {
				let t = e.messageId;
				return r.has(t) ? H(() => new M(`Cannot send 'TEXT_MESSAGE_START' event: A text message with ID '${t}' is already in progress. Complete it with 'TEXT_MESSAGE_END' first.`)) : (r.set(t, !0), V(e));
			}
			case N.TEXT_MESSAGE_CONTENT: {
				let t = e.messageId;
				return r.has(t) ? V(e) : H(() => new M(`Cannot send 'TEXT_MESSAGE_CONTENT' event: No active text message found with ID '${t}'. Start a text message with 'TEXT_MESSAGE_START' first.`));
			}
			case N.TEXT_MESSAGE_END: {
				let t = e.messageId;
				return r.has(t) ? (r.delete(t), V(e)) : H(() => new M(`Cannot send 'TEXT_MESSAGE_END' event: No active text message found with ID '${t}'. A 'TEXT_MESSAGE_START' event must be sent first.`));
			}
			case N.TOOL_CALL_START: {
				let t = e.toolCallId;
				return i.has(t) ? H(() => new M(`Cannot send 'TOOL_CALL_START' event: A tool call with ID '${t}' is already in progress. Complete it with 'TOOL_CALL_END' first.`)) : (i.set(t, !0), V(e));
			}
			case N.TOOL_CALL_ARGS: {
				let t = e.toolCallId;
				return i.has(t) ? V(e) : H(() => new M(`Cannot send 'TOOL_CALL_ARGS' event: No active tool call found with ID '${t}'. Start a tool call with 'TOOL_CALL_START' first.`));
			}
			case N.TOOL_CALL_END: {
				let t = e.toolCallId;
				return i.has(t) ? (i.delete(t), V(e)) : H(() => new M(`Cannot send 'TOOL_CALL_END' event: No active tool call found with ID '${t}'. A 'TOOL_CALL_START' event must be sent first.`));
			}
			case N.STEP_STARTED: {
				let t = e.stepName;
				return c.has(t) ? H(() => new M(`Step "${t}" is already active for 'STEP_STARTED'`)) : (c.set(t, !0), V(e));
			}
			case N.STEP_FINISHED: {
				let t = e.stepName;
				return c.has(t) ? (c.delete(t), V(e)) : H(() => new M(`Cannot send 'STEP_FINISHED' for step "${t}" that was not started`));
			}
			case N.RUN_STARTED: return d = !0, V(e);
			case N.RUN_FINISHED:
				if (c.size > 0) {
					let e = Array.from(c.keys()).join(", ");
					return H(() => new M(`Cannot send 'RUN_FINISHED' while steps are still active: ${e}`));
				}
				if (r.size > 0) {
					let e = Array.from(r.keys()).join(", ");
					return H(() => new M(`Cannot send 'RUN_FINISHED' while text messages are still active: ${e}`));
				}
				if (i.size > 0) {
					let e = Array.from(i.keys()).join(", ");
					return H(() => new M(`Cannot send 'RUN_FINISHED' while tool calls are still active: ${e}`));
				}
				return a = !0, V(e);
			case N.RUN_ERROR: return o = !0, V(e);
			case N.CUSTOM: return V(e);
			case N.THINKING_TEXT_MESSAGE_START: return l ? u ? H(() => new M("Cannot send 'THINKING_TEXT_MESSAGE_START' event: A thinking message is already in progress. Complete it with 'THINKING_TEXT_MESSAGE_END' first.")) : (u = !0, V(e)) : H(() => new M("Cannot send 'THINKING_TEXT_MESSAGE_START' event: A thinking step is not in progress. Create one with 'THINKING_START' first."));
			case N.THINKING_TEXT_MESSAGE_CONTENT: return u ? V(e) : H(() => new M("Cannot send 'THINKING_TEXT_MESSAGE_CONTENT' event: No active thinking message found. Start a message with 'THINKING_TEXT_MESSAGE_START' first."));
			case N.THINKING_TEXT_MESSAGE_END: return u ? (u = !1, V(e)) : H(() => new M("Cannot send 'THINKING_TEXT_MESSAGE_END' event: No active thinking message found. A 'THINKING_TEXT_MESSAGE_START' event must be sent first."));
			case N.THINKING_START: return l ? H(() => new M("Cannot send 'THINKING_START' event: A thinking step is already in progress. End it with 'THINKING_END' first.")) : (l = !0, V(e));
			case N.THINKING_END: return l ? (l = !1, V(e)) : H(() => new M("Cannot send 'THINKING_END' event: No active thinking step found. A 'THINKING_START' event must be sent first."));
			default: return V(e);
		}
	}));
}, ms = function(e) {
	return e.HEADERS = "headers", e.DATA = "data", e;
}({}), hs = (e, t) => Yi(() => Hi(fetch(e, t))).pipe(ea((e) => {
	if (!e.ok) {
		let t = e.headers.get("content-type") || "";
		return Hi(e.text()).pipe(qi((n) => {
			let r = n;
			if (t.includes("application/json")) try {
				r = JSON.parse(n);
			} catch {}
			let i = Error(`HTTP ${e.status}: ${typeof r == "string" ? r : JSON.stringify(r)}`);
			return i.status = e.status, i.payload = r, H(() => i);
		}));
	}
	let t = {
		type: ms.HEADERS,
		status: e.status,
		headers: e.headers
	}, n = e.body?.getReader();
	return n ? new R((e) => (e.next(t), (async () => {
		try {
			for (;;) {
				let { done: t, value: r } = await n.read();
				if (t) break;
				let i = {
					type: ms.DATA,
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
	})) : H(() => Error("Failed to getReader() from response"));
})), gs = (e, t) => {
	let n = cs(t), r = new oi(), i = new TextDecoder("utf-8", { fatal: !1 }), a = "";
	e.subscribe({
		next: (e) => {
			if (e.type !== ms.HEADERS && e.type === ms.DATA && e.data) {
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
}, _s = (e) => {
	let t = new oi(), n = new Uint8Array();
	e.subscribe({
		next: (e) => {
			if (e.type !== ms.HEADERS && e.type === ms.DATA && e.data) {
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
				let r = Xo(n.slice(4, e));
				t.next(r), n = n.slice(e);
			} catch (e) {
				let n = e instanceof Error ? e.message : String(e);
				t.error(Error(`Failed to decode protocol buffer message: ${n}`));
				return;
			}
		}
	}
	return t.asObservable();
}, vs = (e, t) => {
	let n = cs(t), r = new oi(), i = new li(), a = !1;
	return e.subscribe({
		next: (e) => {
			if (i.next(e), e.type === ms.HEADERS && !a) {
				a = !0;
				let t = e.headers.get("content-type");
				n?.lifecycle("HTTP", "Stream format detected:", {
					contentType: t,
					parser: t === "application/vnd.ag-ui.event+proto" ? "protobuf" : "sse"
				}), t === "application/vnd.ag-ui.event+proto" ? _s(i).subscribe({
					next: (e) => r.next(e),
					error: (e) => r.error(e),
					complete: () => r.complete()
				}) : gs(i, n).subscribe({
					next: (e) => {
						try {
							let t = Mn.parse(e);
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
								type: N.RUN_ERROR,
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
}, Z = wt([
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
]), ys = wt([
	"LangGraphInterruptEvent",
	"PredictState",
	"Exit"
]);
St("type", [
	A({
		type: j(Z.enum.TextMessageStart),
		messageId: E(),
		parentMessageId: E().optional(),
		role: E().optional()
	}),
	A({
		type: j(Z.enum.TextMessageContent),
		messageId: E(),
		content: E()
	}),
	A({
		type: j(Z.enum.TextMessageEnd),
		messageId: E()
	}),
	A({
		type: j(Z.enum.ActionExecutionStart),
		actionExecutionId: E(),
		actionName: E(),
		parentMessageId: E().optional()
	}),
	A({
		type: j(Z.enum.ActionExecutionArgs),
		actionExecutionId: E(),
		args: E()
	}),
	A({
		type: j(Z.enum.ActionExecutionEnd),
		actionExecutionId: E()
	}),
	A({
		type: j(Z.enum.ActionExecutionResult),
		actionName: E(),
		actionExecutionId: E(),
		result: E()
	}),
	A({
		type: j(Z.enum.AgentStateMessage),
		threadId: E(),
		agentName: E(),
		nodeName: E(),
		runId: E(),
		active: D(),
		role: E(),
		state: E(),
		running: D()
	}),
	A({
		type: j(Z.enum.MetaEvent),
		name: ys,
		value: O()
	}),
	A({
		type: j(Z.enum.RunError),
		message: E(),
		code: E().optional()
	})
]), A({
	id: E(),
	role: E(),
	content: E(),
	parentMessageId: E().optional()
}), A({
	id: E(),
	name: E(),
	arguments: O(),
	parentMessageId: E().optional()
}), A({
	id: E(),
	result: O(),
	actionExecutionId: E(),
	actionName: E()
});
var bs = (e) => {
	if (typeof e == "string") return e;
	if (!Array.isArray(e)) return;
	let t = e.filter((e) => e.type === "text").map((e) => e.text).filter((e) => e.length > 0);
	if (t.length !== 0) return t.join("\n");
}, xs = (e, t, n) => (r) => {
	let i = {}, a = !0, o = !0, s = "", c = null, l = null, u = [], d = {}, f = (e) => {
		typeof e == "object" && e && ("messages" in e && delete e.messages, i = e);
	};
	return r.pipe(qi((r) => {
		switch (r.type) {
			case N.TEXT_MESSAGE_START: {
				let e = r;
				return [{
					type: Z.enum.TextMessageStart,
					messageId: e.messageId,
					role: e.role
				}];
			}
			case N.TEXT_MESSAGE_CONTENT: {
				let e = r;
				return [{
					type: Z.enum.TextMessageContent,
					messageId: e.messageId,
					content: e.delta
				}];
			}
			case N.TEXT_MESSAGE_END: {
				let e = r;
				return [{
					type: Z.enum.TextMessageEnd,
					messageId: e.messageId
				}];
			}
			case N.TOOL_CALL_START: {
				let e = r;
				return u.push({
					id: e.toolCallId,
					type: "function",
					function: {
						name: e.toolCallName,
						arguments: ""
					}
				}), o = !0, d[e.toolCallId] = e.toolCallName, [{
					type: Z.enum.ActionExecutionStart,
					actionExecutionId: e.toolCallId,
					actionName: e.toolCallName,
					parentMessageId: e.parentMessageId
				}];
			}
			case N.TOOL_CALL_ARGS: {
				let c = r, d = u.find((e) => e.id === c.toolCallId);
				if (!d) return console.warn(`TOOL_CALL_ARGS: No tool call found with ID '${c.toolCallId}'`), [];
				d.function.arguments += c.delta;
				let p = !1;
				if (l) {
					let e = l.find((e) => e.tool == d.function.name);
					if (e) try {
						let t = JSON.parse(ia(d.function.arguments));
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
					type: Z.enum.ActionExecutionArgs,
					actionExecutionId: c.toolCallId,
					args: c.delta
				}, ...p ? [{
					type: Z.enum.AgentStateMessage,
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
			case N.TOOL_CALL_END: {
				let e = r;
				return [{
					type: Z.enum.ActionExecutionEnd,
					actionExecutionId: e.toolCallId
				}];
			}
			case N.TOOL_CALL_RESULT: {
				let e = r;
				return [{
					type: Z.enum.ActionExecutionResult,
					actionExecutionId: e.toolCallId,
					result: e.content,
					actionName: d[e.toolCallId] || "unknown"
				}];
			}
			case N.RAW: return [];
			case N.CUSTOM: {
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
					type: Z.enum.MetaEvent,
					name: e.name,
					value: e.value
				}];
			}
			case N.STATE_SNAPSHOT: return f(r.snapshot), [{
				type: Z.enum.AgentStateMessage,
				threadId: e,
				agentName: n,
				nodeName: s,
				runId: t,
				running: a,
				role: "assistant",
				state: JSON.stringify(i),
				active: o
			}];
			case N.STATE_DELTA: {
				let c = r, l = fr.applyPatch(i, c.delta, !0, !1);
				return l ? (f(l.newDocument), [{
					type: Z.enum.AgentStateMessage,
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
			case N.MESSAGES_SNAPSHOT: return c = r.messages, [{
				type: Z.enum.AgentStateMessage,
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
			case N.RUN_STARTED: return [];
			case N.RUN_FINISHED: return c && (i.messages = c), Object.keys(i).length === 0 ? [] : [{
				type: Z.enum.AgentStateMessage,
				threadId: e,
				agentName: n,
				nodeName: s,
				runId: t,
				running: a,
				role: "assistant",
				state: JSON.stringify({
					...i,
					...c ? { messages: Ss(c) } : {}
				}),
				active: !1
			}];
			case N.RUN_ERROR: {
				let e = r;
				return [{
					type: Z.enum.RunError,
					message: e.message,
					code: e.code
				}];
			}
			case N.STEP_STARTED: return s = r.stepName, u = [], l = null, [{
				type: Z.enum.AgentStateMessage,
				threadId: e,
				agentName: n,
				nodeName: s,
				runId: t,
				running: a,
				role: "assistant",
				state: JSON.stringify(i),
				active: !0
			}];
			case N.STEP_FINISHED: return u = [], l = null, [{
				type: Z.enum.AgentStateMessage,
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
function Ss(e) {
	let t = [];
	for (let n of e) if (n.role === "assistant" || n.role === "user" || n.role === "system") {
		let e = bs(n.content);
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
var Cs = (e) => (t) => {
	let n = cs(e), r, i, a, o, s = () => {
		if (!r || o !== "text") throw Error("No text message to close");
		let e = {
			type: N.TEXT_MESSAGE_END,
			messageId: r.messageId
		};
		return o = void 0, r = void 0, n?.event("TRANSFORM", "TEXT_MESSAGE_END", e, { messageId: e.messageId }), e;
	}, c = () => {
		if (!i || o !== "tool") throw Error("No tool call to close");
		let e = {
			type: N.TOOL_CALL_END,
			toolCallId: i.toolCallId
		};
		return o = void 0, i = void 0, n?.event("TRANSFORM", "TOOL_CALL_END", e, { toolCallId: e.toolCallId }), e;
	}, l = () => {
		if (!a || o !== "reasoning") throw Error("No reasoning message to close");
		let e = {
			type: N.REASONING_MESSAGE_END,
			messageId: a.messageId
		};
		return o = void 0, a = void 0, n?.event("TRANSFORM", "REASONING_MESSAGE_END", e, { messageId: e.messageId }), e;
	}, u = () => o === "text" ? [s()] : o === "tool" ? [c()] : o === "reasoning" ? [l()] : [];
	return t.pipe(qi((e) => {
		switch (e.type) {
			case N.TEXT_MESSAGE_START:
			case N.TEXT_MESSAGE_CONTENT:
			case N.TEXT_MESSAGE_END:
			case N.TOOL_CALL_START:
			case N.TOOL_CALL_ARGS:
			case N.TOOL_CALL_END:
			case N.TOOL_CALL_RESULT:
			case N.STATE_SNAPSHOT:
			case N.STATE_DELTA:
			case N.MESSAGES_SNAPSHOT:
			case N.CUSTOM:
			case N.RUN_STARTED:
			case N.RUN_FINISHED:
			case N.RUN_ERROR:
			case N.STEP_STARTED:
			case N.STEP_FINISHED:
			case N.THINKING_START:
			case N.THINKING_END:
			case N.THINKING_TEXT_MESSAGE_START:
			case N.THINKING_TEXT_MESSAGE_CONTENT:
			case N.THINKING_TEXT_MESSAGE_END:
			case N.REASONING_START:
			case N.REASONING_MESSAGE_START:
			case N.REASONING_MESSAGE_CONTENT:
			case N.REASONING_MESSAGE_END:
			case N.REASONING_END: return [...u(), e];
			case N.RAW:
			case N.ACTIVITY_SNAPSHOT:
			case N.ACTIVITY_DELTA:
			case N.REASONING_ENCRYPTED_VALUE: return [e];
			case N.TEXT_MESSAGE_CHUNK:
				let t = e, s = [];
				if ((o !== "text" || t.messageId !== void 0 && t.messageId !== r?.messageId) && s.push(...u()), o !== "text") {
					if (t.messageId === void 0) throw Error("First TEXT_MESSAGE_CHUNK must have a messageId");
					r = {
						messageId: t.messageId,
						name: t.name
					}, o = "text";
					let e = {
						type: N.TEXT_MESSAGE_START,
						messageId: t.messageId,
						role: t.role || "assistant",
						...t.name !== void 0 && { name: t.name }
					};
					s.push(e), n?.event("TRANSFORM", "TEXT_MESSAGE_START", e, { messageId: t.messageId });
				}
				if (t.delta !== void 0) {
					let e = {
						type: N.TEXT_MESSAGE_CONTENT,
						messageId: r.messageId,
						delta: t.delta
					};
					s.push(e), n?.event("TRANSFORM", "TEXT_MESSAGE_CONTENT", e, { messageId: r.messageId });
				}
				return s;
			case N.TOOL_CALL_CHUNK:
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
						type: N.TOOL_CALL_START,
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
						type: N.TOOL_CALL_ARGS,
						toolCallId: i.toolCallId,
						delta: c.delta
					};
					l.push(e), n?.event("TRANSFORM", "TOOL_CALL_ARGS", e, { toolCallId: i.toolCallId });
				}
				return l;
			case N.REASONING_MESSAGE_CHUNK:
				let d = e, f = [];
				if ((o !== "reasoning" || d.messageId && d.messageId !== a?.messageId) && f.push(...u()), o !== "reasoning") {
					if (d.messageId === void 0) throw Error("First REASONING_MESSAGE_CHUNK must have a messageId");
					a = { messageId: d.messageId }, o = "reasoning";
					let e = {
						type: N.REASONING_MESSAGE_START,
						messageId: d.messageId
					};
					f.push(e), n?.event("TRANSFORM", "REASONING_MESSAGE_START", e, { messageId: d.messageId });
				}
				if (d.delta !== void 0) {
					let e = {
						type: N.REASONING_MESSAGE_CONTENT,
						messageId: a.messageId,
						delta: d.delta
					};
					f.push(e), n?.event("TRANSFORM", "REASONING_MESSAGE_CONTENT", e, { messageId: a.messageId });
				}
				return f;
		}
		return e.type, [];
	}), $i(() => {
		u();
	}));
}, ws = class {
	runNext(e, t) {
		return t.run(e).pipe(Cs(!1));
	}
	runNextWithState(e, t) {
		let n = Y(e.messages || []), r = Y(e.state || {}), i = new li();
		return fs(e, i, t, []).subscribe((e) => {
			e.messages !== void 0 && (n = e.messages), e.state !== void 0 && (r = e.state);
		}), this.runNext(e, t).pipe(Zi(async (e) => (i.next(e), await new Promise((e) => setTimeout(e, 0)), {
			event: e,
			messages: Y(n),
			state: Y(r)
		})));
	}
}, Ts = class extends ws {
	constructor(e) {
		super(), this.fn = e;
	}
	run(e, t) {
		return this.fn(e, t);
	}
};
function Es(e) {
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
var Ds = class extends ws {
	run(e, t) {
		let { parentRunId: n, ...r } = e, i = {
			...r,
			messages: r.messages.map(Es)
		};
		return this.runNext(i, t);
	}
}, Os = "THINKING_START", ks = "THINKING_END", As = "THINKING_TEXT_MESSAGE_START", js = "THINKING_TEXT_MESSAGE_CONTENT", Ms = "THINKING_TEXT_MESSAGE_END", Ns = class extends ws {
	constructor(...e) {
		super(...e), this.currentReasoningId = null, this.currentMessageId = null;
	}
	warnAboutTransformation(e, t) {
		typeof process < "u" && process.env !== void 0 && process.env.SUPPRESS_TRANSFORMATION_WARNINGS || console.warn(`AG-UI is converting ${e} to ${t}. To remove this warning, upgrade your AG-UI integration package (e.g. @ag-ui/langgraph). To surpress it, set SUPPRESS_TRANSFORMATION_WARNINGS=true in your .env file.`);
	}
	run(e, t) {
		return this.currentReasoningId = null, this.currentMessageId = null, this.runNext(e, t).pipe(Gi((e) => this.transformEvent(e)));
	}
	transformEvent(e) {
		switch (e.type) {
			case Os: {
				this.currentReasoningId = as();
				let { title: t, ...n } = e;
				return this.warnAboutTransformation(Os, N.REASONING_START), {
					...n,
					type: N.REASONING_START,
					messageId: this.currentReasoningId
				};
			}
			case As: return this.currentMessageId = as(), this.warnAboutTransformation(As, N.REASONING_MESSAGE_START), {
				...e,
				type: N.REASONING_MESSAGE_START,
				messageId: this.currentMessageId,
				role: "assistant"
			};
			case js: {
				let { delta: t, ...n } = e;
				return this.warnAboutTransformation(js, N.REASONING_MESSAGE_CONTENT), {
					...n,
					type: N.REASONING_MESSAGE_CONTENT,
					messageId: this.currentMessageId ?? as(),
					delta: t
				};
			}
			case Ms: {
				let t = this.currentMessageId ?? as();
				return this.warnAboutTransformation(Ms, N.REASONING_MESSAGE_END), {
					...e,
					type: N.REASONING_MESSAGE_END,
					messageId: t
				};
			}
			case ks: {
				let t = this.currentReasoningId ?? as();
				return this.warnAboutTransformation(ks, N.REASONING_END), {
					...e,
					type: N.REASONING_END,
					messageId: t
				};
			}
			default: return e;
		}
	}
};
function Ps(e) {
	return e.startsWith("image/") ? "image" : e.startsWith("audio/") ? "audio" : e.startsWith("video/") ? "video" : "document";
}
function Fs(e) {
	return typeof e == "object" && !!e && "type" in e && e.type === "binary" && "mimeType" in e && typeof e.mimeType == "string";
}
function Is(e) {
	let t = Ps(e.mimeType);
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
function Ls(e) {
	let t = e.content;
	if (!Array.isArray(t)) return e;
	let n = t.map((e) => Fs(e) ? Is(e) : e);
	return {
		...e,
		content: n
	};
}
var Rs = class extends ws {
	run(e, t) {
		let n = {
			...e,
			messages: e.messages.map(Ls)
		};
		return this.runNext(n, t);
	}
}, zs = "0.0.53", Bs = class {
	get maxVersion() {
		return zs;
	}
	get debug() {
		return this._debug;
	}
	set debug(e) {
		this._debug = ss(e), this._debugLogger = us(this._debug);
	}
	get debugLogger() {
		return this._debugLogger;
	}
	set debugLogger(e) {
		typeof e == "boolean" ? this._debugLogger = e ? us(ss(!0)) : void 0 : this._debugLogger = e;
	}
	constructor({ agentId: e, description: t, threadId: n, initialMessages: r, initialState: i, debug: a } = {}) {
		this.subscribers = [], this.isRunning = !1, this.middlewares = [], this.agentId = e, this.description = t ?? "", this.threadId = n ?? c(), this.messages = Y(r ?? []), this.state = Y(i ?? {}), this._debug = ss(a), this._debugLogger = us(this._debug), is(this.maxVersion, "0.0.39") <= 0 && this.middlewares.unshift(new Ds()), is(this.maxVersion, "0.0.45") <= 0 && this.middlewares.unshift(new Ns()), is(this.maxVersion, "0.0.47") <= 0 && this.middlewares.unshift(new Rs());
	}
	subscribe(e) {
		return this.subscribers.push(e), { unsubscribe: () => {
			this.subscribers = this.subscribers.filter((t) => t !== e);
		} };
	}
	use(...e) {
		let t = e.map((e) => typeof e == "function" ? new Ts(e) : e);
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
			await this.onInitialize(n, a), this.activeRunDetach$ = new oi();
			let o;
			this.activeRunCompletionPromise = new Promise((e) => {
				o = e;
			}), await Wi(Qr(() => this.middlewares.length === 0 ? this.run(n) : this.middlewares.reduceRight((e, t) => ({
				run: (n) => t.run(n, e),
				get messages() {
					return e.messages;
				},
				get state() {
					return e.state;
				}
			}), this).run(n), Cs(this.debugLogger), ps(this.debugLogger), (e) => e.pipe(ta(this.activeRunDetach$)), (e) => this.apply(n, e, a), (e) => this.processApplyEvents(n, e, a), Xi((e) => (this.debugLogger?.lifecycle("LIFECYCLE", "Run errored:", {
				agentId: this.agentId,
				error: e instanceof Error ? e.message : String(e)
			}), this.isRunning = !1, this.onError(n, e, a))), $i(() => {
				this.debugLogger?.lifecycle("LIFECYCLE", "Run finished:", {
					agentId: this.agentId,
					threadId: this.threadId
				}), this.isRunning = !1, this.onFinalize(n, a), o?.(), o = void 0, this.activeRunCompletionPromise = void 0, this.activeRunDetach$ = void 0;
			}))(V(null)));
			let s = Y(this.messages).filter((e) => !i.has(e.id));
			return {
				result: r,
				newMessages: s
			};
		} finally {
			this.isRunning = !1;
		}
	}
	connect(e) {
		throw new Ut();
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
			await this.onInitialize(n, a), this.activeRunDetach$ = new oi();
			let o;
			this.activeRunCompletionPromise = new Promise((e) => {
				o = e;
			}), await Wi(Qr(() => Yi(() => this.connect(n)), Cs(this.debugLogger), ps(this.debugLogger), (e) => e.pipe(ta(this.activeRunDetach$)), (e) => this.apply(n, e, a), (e) => this.processApplyEvents(n, e, a), Xi((e) => (this.isRunning = !1, e instanceof Ut ? ui : this.onError(n, e, a))), $i(() => {
				this.isRunning = !1, this.onFinalize(n, a), o?.(), o = void 0, this.activeRunCompletionPromise = void 0, this.activeRunDetach$ = void 0;
			}))(V(null)), { defaultValue: void 0 });
			let s = Y(this.messages).filter((e) => !i.has(e.id));
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
		return fs(e, t, this, n, this.debugLogger);
	}
	processApplyEvents(e, t, n) {
		return t.pipe(na((t) => {
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
		let t = Y(this.messages).filter((e) => e.role !== "activity");
		return {
			threadId: this.threadId,
			runId: e?.runId || c(),
			tools: Y(e?.tools ?? []),
			context: Y(e?.context ?? []),
			forwardedProps: Y(e?.forwardedProps ?? {}),
			state: Y(this.state),
			messages: t
		};
	}
	async onInitialize(e, t) {
		let n = await X(t, this.messages, this.state, (t, n, r) => t.onRunInitialized?.({
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
		return Hi(X(n, this.messages, this.state, (n, r, i) => n.onRunFailed?.({
			error: t,
			messages: r,
			state: i,
			agent: this,
			input: e
		}))).pipe(Gi((r) => {
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
		let n = await X(t, this.messages, this.state, (t, n, r) => t.onRunFinalized?.({
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
		return e.agentId = this.agentId, e.description = this.description, e.threadId = this.threadId, e.messages = Y(this.messages), e.state = Y(this.state), e._debug = this._debug, e._debugLogger = this._debugLogger, e.isRunning = this.isRunning, e.subscribers = [...this.subscribers], e.middlewares = [...this.middlewares], e;
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
		this.messages = Y(e), (async () => {
			for (let e of this.subscribers) await e.onMessagesChanged?.({
				messages: this.messages,
				state: this.state,
				agent: this
			});
		})();
	}
	setState(e) {
		this.state = Y(e), (async () => {
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
		}), this).run(t)).pipe(Cs(this.debugLogger), ps(this.debugLogger), xs(this.threadId, t.runId, this.agentId), (e) => e.pipe(Gi((e) => (this.debugLogger?.event("LEGACY", "Event:", e, { type: e.type }), e))));
	}
}, Vs = class extends Bs {
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
		super(e), this.abortController = new AbortController(), this.url = e.url, this.headers = Y(e.headers ?? {});
	}
	run(e) {
		return vs(hs(this.url, this.requestInit(e)), this.debugLogger);
	}
	clone() {
		let e = super.clone();
		e.url = this.url, e.headers = Y(this.headers ?? {});
		let t = new AbortController(), n = this.abortController.signal;
		return n.aborted && t.abort(n.reason), e.abortController = t, e;
	}
}, Q = document.getElementById("messages"), $ = document.getElementById("input"), Hs = document.getElementById("send"), Us = document.getElementById("attach-btn"), Ws = document.getElementById("file-input"), Gs = document.getElementById("attachment-preview-container"), Ks = localStorage.getItem("ag-ui-thread-id") ?? crypto.randomUUID();
localStorage.setItem("ag-ui-thread-id", Ks);
var qs = [], Js = [];
Us.addEventListener("click", () => Ws.click()), Ws.addEventListener("change", () => {
	if (Ws.files) {
		for (let e of Array.from(Ws.files)) Js.push(e);
		Ys(), Ws.value = "";
	}
});
function Ys() {
	Gs.innerHTML = "", Js.forEach((e, t) => {
		let n = document.createElement("div");
		n.className = "file-preview-badge", n.innerHTML = `<span>&#128196; ${e.name}</span><span class="remove-file" data-index="${t}">&times;</span>`, n.querySelector(".remove-file").addEventListener("click", (e) => {
			let t = parseInt(e.target.dataset.index);
			Js.splice(t, 1), Ys();
		}), Gs.appendChild(n);
	});
}
function Xs(e) {
	return new Promise((t, n) => {
		let r = new FileReader();
		r.onload = () => t(r.result), r.onerror = (e) => n(e), r.readAsDataURL(e);
	});
}
function Zs(e, t) {
	let n = document.createElement("div");
	return n.className = `msg ${e}`, e === "assistant" && t === "…" ? n.innerHTML = "<div class=\"loading-wave\"><span class=\"dot\">.</span><span class=\"dot\">.</span><span class=\"dot\">.</span></div>" : n.textContent = t, Q.appendChild(n), Q.scrollTop = Q.scrollHeight, n;
}
function Qs() {
	for (let e of qs) e.role === "user" && typeof e.content == "string" ? Zs("user", e.content) : e.role === "assistant" && typeof e.content == "string" && e.content.length > 0 && Zs("assistant", e.content);
}
async function $s() {
	let e = await fetch(`/history/${encodeURIComponent(Ks)}`);
	if (!e.ok) return;
	let t = await e.json(), n = /* @__PURE__ */ new Set();
	for (let e of t) {
		let t = e.messageId ?? crypto.randomUUID();
		if (n.has(t)) continue;
		n.add(t);
		let r = e.contents.find((e) => e.$type === "text")?.text;
		r && (e.role === "user" ? qs.push({
			id: t,
			role: "user",
			content: r
		}) : e.role === "assistant" && qs.push({
			id: t,
			role: "assistant",
			content: r
		}));
	}
	qs.length > 0 && Qs();
}
var ec = /* @__PURE__ */ new Map();
function tc(e) {
	let t = document.createElement("div");
	t.className = "msg approval-request", t.dataset.toolCallId = e.toolCallId, t.innerHTML = `
		<div class="approval-header"><span>&#9888;</span> Approval Required</div>
		<div class="approval-tool">Tool: <code>${e.functionName}</code></div>
		<div class="approval-actions">
			<button class="approve-btn">Approve</button>
			<button class="deny-btn">Deny</button>
		</div>`, t.querySelector(".approve-btn").addEventListener("click", () => {
		t.remove(), ec.delete(e.toolCallId), e.resolve(!0);
	}), t.querySelector(".deny-btn").addEventListener("click", () => {
		t.remove(), ec.delete(e.toolCallId), e.resolve(!1);
	}), Q.appendChild(t), Q.scrollTop = Q.scrollHeight;
}
async function nc(e, t) {
	let n = new Vs({ url: "/agui" }), r = t.querySelector(".loading-wave") ? "" : t.textContent ?? "", i = /* @__PURE__ */ new Map(), a = /* @__PURE__ */ new Map(), o = [];
	await new Promise((s, c) => {
		n.run(e).subscribe({
			next: (n) => {
				switch (n.type) {
					case N.TEXT_MESSAGE_CONTENT:
						r += n.delta ?? "", t.textContent = r, Q.scrollTop = Q.scrollHeight;
						break;
					case N.RUN_ERROR: {
						let e = n;
						t.classList.add("error"), t.innerHTML = `
							<div class="error-header"><span>&#10060;</span> Run Error</div>
							${e.code ? `<div class="error-detail">Code: <code>${e.code}</code></div>` : ""}
							<div class="error-detail">${e.message}</div>`, Q.scrollTop = Q.scrollHeight;
						break;
					}
					case N.TOOL_CALL_START: {
						let e = n;
						a.set(e.toolCallId, e.toolCallName), i.set(e.toolCallId, "");
						break;
					}
					case N.TOOL_CALL_ARGS: {
						let e = n;
						i.set(e.toolCallId, (i.get(e.toolCallId) ?? "") + e.delta);
						break;
					}
					case N.TOOL_CALL_END: {
						let r = n.toolCallId, s = a.get(r) ?? "", c = i.get(r) ?? "{}";
						if (s !== "request_approval") break;
						o.push({
							id: r,
							name: s,
							args: c
						});
						let l = r, u = "unknown", d = JSON.parse(c);
						if (d.request) {
							let e = JSON.parse(d.request);
							l = e.approval_id ?? r, u = e.function_name ?? "unknown";
						}
						let f = {
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
								await nc({
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
						ec.set(r, f), tc(f);
						break;
					}
				}
			},
			error: (e) => {
				t.classList.add("error"), t.innerHTML = `
					<div class="error-header"><span>&#10060;</span> Connection Error</div>
					<div class="error-detail">${e}</div>`, Hs.disabled = !1, c(e);
			},
			complete: () => s()
		});
	});
}
async function rc() {
	let e = $.value.trim();
	if (!e && Js.length === 0) return;
	$.value = "", ic(), Hs.disabled = !0, Us.disabled = !0;
	let t = [];
	e && t.push({
		type: "text",
		text: e
	});
	let n = [];
	for (let e of Js) {
		n.push(e.name);
		let r = await Xs(e), i = e.type || "application/octet-stream", a = {
			type: "url",
			value: r,
			mimeType: i
		};
		i.startsWith("image/") ? t.push({
			type: "image",
			source: a
		}) : t.push({
			type: "document",
			source: a
		});
	}
	e + (n.length > 0 ? `\n\n[Attached: ${n.join(", ")}]` : "");
	let r = Js.length > 0 ? `${e} [File Attached]` : e;
	Js = [], Gs.innerHTML = "";
	let i = {
		id: crypto.randomUUID(),
		role: "user",
		content: e
	};
	qs.push(i), Zs("user", r);
	let a = Zs("assistant", "…"), o = {
		threadId: Ks,
		runId: crypto.randomUUID(),
		messages: [i],
		tools: [],
		context: []
	};
	try {
		await nc(o, a), a.textContent && a.textContent !== "…" && qs.push({
			id: crypto.randomUUID(),
			role: "assistant",
			content: a.textContent
		});
	} finally {
		Hs.disabled = !1, Us.disabled = !1, $.focus();
	}
}
function ic() {
	$.style.height = "0";
	let e = $.scrollHeight, t = parseFloat(getComputedStyle($).maxHeight);
	e >= t ? ($.style.height = `${t}px`, $.style.overflowY = "auto") : ($.style.height = `${e}px`, $.style.overflowY = "hidden");
}
Hs.addEventListener("click", rc), $.addEventListener("keydown", (e) => {
	e.key === "Enter" && (e.shiftKey ? setTimeout(ic, 0) : (e.preventDefault(), rc()));
}), $.addEventListener("input", ic), $s(), $.focus();
//#endregion
